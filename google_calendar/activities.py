import json
import pytz
import time
import datetime
import calendar
import dateutil.parser

# Local imports
import store_utils
import scheduler_utils
import settings
from google_calendar_backend import *

logging.config.dictConfig(settings.LOGGING)
logger = logging.getLogger("google_calendar")


def sync_for_calendar(calendar_name):
    logger.debug("[google_calendar] Synchronizing activities for calendar '%s' ... " % calendar_name)

    user = store_utils.user_get_by_calendar(calendar_name)
    if not user:
        logger.error(
            "[google_calendar.activities] FAILED to retrieve any user info for calendar" % calendar_name)
        return

    logger.debug("[google_calendar] Synchronizing activities from calendar '%s' for user '%s' ... " % (
        calendar_name, user['username']))

    # Hardcoded calendars for demo
    # calendars = {
    #     "personal": "7eh6qnivid6430dl79ei89k26g@group.calendar.google.com",
    #     "exercise": "8puar0sc4e7efns5r849rn0lus@group.calendar.google.com",
    #     "medication": "us8v5j6ttp885542q9o2aljrho@group.calendar.google.com"
    # }
    calendars = settings.CALENDAR_IDs[calendar_name]

    logger.debug("[google_calendar] Getting the Google Calendar service... ")

    # Get calendar service for the current user
    calendar_credentials = get_credentials(calendar_name)
    calendar_service = get_calendar_service(calendar_credentials)

    logger.debug("[google_calendar] Successfully got Google Calendar service!")

    for calendar_type, calendar_id in calendars.iteritems():
        if not calendar_id:
            logger.debug("[google_calendar] Calendar id for the '%s' activity type is empty!" % calendar_type)
            continue

        logger.debug("[google_calendar] Getting the calendar with id '%s' for '%s' activity type ..." % (
            calendar_id,
            calendar_type
        ))

        # Get the calendar
        calendar, res_code = get_calendar(calendar_service, calendar_id)

        if res_code == 200:
            logger.debug("[google_calendar] Successfully got calendar: %s" % str(calendar))
        else:
            logger.error("[google_calendar] Failed to get calendar! Response code: %s" % str(res_code))
            continue

        # Calculate timeframe for fetching events
        tz = pytz.timezone(calendar['timeZone'])
        date_from = datetime.datetime.now(tz) - datetime.timedelta(days=7)
        date_to = datetime.datetime.now(tz) + datetime.timedelta(days=7)

        logger.debug("[google_calendar] Getting events starting from %s to %s ..." % (
            str(date_from),
            str(date_to)
        ))

        # Get events for the current calendar
        events = list_activities(calendar_service, calendar_id, date_from, date_to)

        logger.debug("[google_calendar] Successfully got %d events from the calendar!" % len(events))

        # Add the activity type to the calendar object
        calendar['activity_type'] = calendar_type

        # Process the collected events
        logger.debug("[google_calendar] Processing the collected events ...")
        process_events(user, calendar, events, date_from, date_to)

    logger.debug("[google_calendar] Finished synchronizing activities for user '%s'!" % user['username'])


def process_events(user, calendar, events, date_from, date_to):
    # Compose the color object
    calendar_colors = {
        'foreground': calendar['foregroundColor'],
        'background': calendar['backgroundColor'],
        'id': calendar['colorId']
    }

    logger.debug("[google_calendar] Getting the activities from Store to compare them with the fetched events ...")
    db_events = store_utils.activity_get(
        user=user['id'],
        calendar_id=calendar['id']
    )

    if db_events != False:
        logger.debug("[google_calendar] Successfully got %d activities from Store!" % len(db_events))
    else:
        logger.debug("[google_calendar] Failed getting activities from Store! Stopped processing the events!")
        return

    # Compute a hash of update times by event id
    db_events_hash = {}
    for db_event in db_events:
        event_id = db_event['event_id']
        db_events_hash[event_id] = {
            'id': db_event['id'],
            'updated': db_event['updated'],
            'color': db_event['color']
        }

    # Process the events
    for event in events['items']:
        activity_data = compose_activity_data(
            event,
            calendar,
            calendar_colors,
            user
        )

        if event['reminders']['useDefault']:
            activity_data['reminders'] = process_event_reminders(
                activity_data['start'],
                events['defaultReminders']
            )
        elif 'overrides' in event['reminders']:
            activity_data['reminders'] = process_event_reminders(
                activity_data['start'],
                event['reminders']['overrides']
            )

        if event['id'] in db_events_hash:
            db_event = db_events_hash[event['id']]

            # Remove the DB event from the hash because it is valid
            db_events_hash.pop(event['id'], None)

            # The calendar color might have changed, add it to the event
            # in order to be compared with the activity color
            event['color'] = calendar_colors

            # If the event does not differ from the already stored
            # activity then continue with the next event
            if event_equals_activity(event, db_event) == True:
                continue

            # The event differs from the already stored activity so add
            # the id field in order for the activity entry to be updated
            activity_data['id'] = db_event['id']

        if 'id' in activity_data:
            logger.debug("[google_calendar] Updating activity from event: %s" % str(event))
        else:
            logger.debug("[google_calendar] Inserting new activity from event: %s" % str(event))

        # Delete existing activity from Scheduler
        scheduler_utils.activity_delete(**activity_data)

        # Add the new / updated activity to Scheduler
        scheduler_utils.activity_post(**activity_data)

        # Get updated data for this activity from Scheduler
        schedule = scheduler_utils.activity_schedule_get()
        logger.debug("[smart_scheduler] Getting the schedule: %s" % str(schedule))

        activity_data_dict = next((activity for activity in schedule if activity['uuid'] == activity_data['event_id']),
                                  None)
        activity_period = activity_data_dict['activityPeriod']

        # Update activity timestamps before adding it to Store
        activity_data['start'] = activity_period
        activity_data['end'] = scheduler_utils.add_duration_to_timestamp(activity_period, activity_data_dict[
            'activityDurationInMinutes'])

        if store_utils.activity_save(**activity_data):
            logger.debug("[google_calendar] Successfully updated/inserted activity!")
        else:
            logger.debug("[google_calendar] Failed updating/inserting activity!")

    # Delete the cancelled DB events (the ones remaining in the hash)
    activities_to_delete = map(
        lambda x: x['id'],
        db_events_hash.values()
    )

    logger.debug("[google_calendar] Deleting activities that do not exist anymore in Google Calendar: %s" % str(
        activities_to_delete))
    if activities_to_delete:
        if store_utils.activity_delete(id__in=activities_to_delete):
            logger.debug("[google_calendar] Successfully deleted activities!")
        else:
            logger.debug("[google_calendar] Failed deleting activities!")


def compose_activity_data(event, calendar, calendar_colors, user):
    activity_data = {
        'event_id': event['id'],
        'user': user['resource_uri'],
        'status': event['status'],
        'html_link': event['htmlLink'],
        'title': event['summary'],
        'calendar_id': calendar['id'],
        'calendar_name': calendar['summary'],
        'created': timestamp_from_event_date(event['created']),
        'updated': timestamp_from_event_date(event['updated']),
        'activity_type': calendar['activity_type'],
        'color': calendar_colors,
        'creator': event['creator'],
        'iCalUID': event['iCalUID']
    }

    if 'dateTime' in event['start']:
        activity_data['start'] = timestamp_from_event_date(event['start']['dateTime'])
    else:
        activity_data['start'] = timestamp_from_event_date(event['start']['date'])

    if 'dateTime' in event['end']:
        activity_data['end'] = timestamp_from_event_date(event['end']['dateTime'])
    else:
        activity_data['end'] = timestamp_from_event_date(event['end']['date'])

    if 'description' in event:
        activity_data['description'] = event['description']

    if 'location' in event:
        activity_data['location'] = event['location']

    if 'recurringEventId' in event:
        activity_data['recurring_event_id'] = event['recurringEventId']

    return activity_data


def event_equals_activity(event, activity):
    # Compare color
    if event['color'] != activity['color']:
        return False

    # Compare update time
    event_updated_timestamp = timestamp_from_event_date(event['updated'])
    if event_updated_timestamp != activity['updated']:
        return False

    return True


def timestamp_from_event_date(date):
    return calendar.timegm(
        dateutil.parser.parse(date).utctimetuple()
    )


def process_event_reminders(start_timestamp, raw_reminders):
    reminders = []

    for r in raw_reminders:
        reminders.append(str(start_timestamp - r['minutes'] * 60))

    return reminders

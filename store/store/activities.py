import pytz
import time
import datetime
import calendar
import dateutil.parser

from .google_calendar_backend import *
from .models import User, Activity


def sync_for_user(user):
    logger.debug("[sync-activities] Synchronizing activities for user '%s' ... " % user.username)

    # Hardcode calendars for demo
    calendars = {
        "personal": "7eh6qnivid6430dl79ei89k26g@group.calendar.google.com",
        "exercise": "8puar0sc4e7efns5r849rn0lus@group.calendar.google.com",
        "medication": "us8v5j6ttp885542q9o2aljrho@group.calendar.google.com",
        "measurement": ""
    }

    logger.debug("[sync-activities] Getting the Google Calendar service... ")

    # Get calendar service
    calendar_credentials = get_credentials()
    calendar_service = get_calendar_service(calendar_credentials)

    logger.debug("[sync-activities] Successfully got Google Calendar service!")

    for calendar_type, calendar_id in calendars.iteritems():
        if not calendar_id:
            logger.debug("[sync-activities] Calendar id for the '%s' activity type is empty!" % calendar_type)
            continue

        logger.debug("[sync-activities] Getting the calendar with id '%s' for '%s' activity type ..." % (
            calendar_id,
            calendar_type
        ))

        # Get the calendar
        calendar, res_code = get_calendar(calendar_service, calendar_id)

        if res_code == 200:
            logger.debug("[sync-activities] Successfully got calendar: %s" % str(calendar))
        else:
            logger.error("[sync-activities] Failed to get calendar! Response code: %s" % str(res_code))
            continue

        # Calculate timeframe for fetching events
        tz = pytz.timezone(calendar['timeZone'])
        date_from = datetime.datetime.now(tz) - datetime.timedelta(days=7)
        date_to = datetime.datetime.now(tz) + datetime.timedelta(days=7)

        logger.debug("[sync-activities] Getting events starting from %s to %s ..." % (
            str(date_from),
            str(date_to)
        ))

        # Get events for the current calendar
        events = list_activities(calendar_service, calendar_id, date_from, date_to)

        logger.debug("[sync-activities] Got %d events from the calendar!" % len(events))

        # Add the activity type to the calendar object
        calendar['activity_type'] = calendar_type

        # Process the collected events
        logger.debug("[sync-activities] Processing the collected events ...")
        process_events(user, calendar, events, date_from, date_to)

    logger.debug("[sync-activities] Finished synchronizing activities for user '%s'!" % user.username)

def process_events(user, calendar, events, date_from, date_to):
    # Compose the color object
    calendar_colors = {
        'foreground': calendar['foregroundColor'],
        'background': calendar['backgroundColor'],
        'id': calendar['colorId']
    }

    # Get the events from DB to compare them with the fetched ones
    db_events = Activity.objects.all().filter(
        start__gte=time.mktime(date_from.timetuple()),
        end__lte=time.mktime(date_to.timetuple()),
        user=user,
        calendar_id=calendar['id']
    )

    # Compute a hash of update times by event id
    db_events_hash = {}
    for db_event in db_events:
        event_id = db_event.event_id
        db_events_hash[event_id] = {
            'id': db_event.id,
            'updated': db_event.updated,
            'color': db_event.color
        }

    # Process the events
    for event in events:
        activity_data = compose_activity_data(
            event,
            calendar,
            calendar_colors,
            user
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
            logger.debug("[sync-activities] Updating activity from event: %s" % str(event))
        else:
            logger.debug("[sync-activities] Inserting new activity from event: %s" % str(event))

        Activity(**activity_data).save()

    # Delete the cancelled DB events (the ones remaining in the hash)
    activities_to_delete = map(
        lambda x: x['id'],
        db_events_hash.values()
    )

    logger.debug("[sync-activities] Deleting activities that do not exist anymore in Google Calendar: %s" % str(activities_to_delete))

    Activity.objects.filter(id__in=activities_to_delete).delete()

def compose_activity_data(event, calendar, calendar_colors, user):
    activity_data = {
        'event_id': event['id'],
        'user': user,
        'status': event['status'],
        'html_link': event['htmlLink'],
        'title': event['summary'],
        'calendar_id': calendar['id'],
        'calendar_name': calendar['summary'],
        'start': timestamp_from_event_date(event['start']['dateTime']),
        'end': timestamp_from_event_date(event['end']['dateTime']),
        'created': timestamp_from_event_date(event['created']),
        'updated': timestamp_from_event_date(event['updated']),
        'activity_type': calendar['activity_type'],
        'color': calendar_colors,
        'creator': event['creator'],
        'iCalUID': event['iCalUID']
    }

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


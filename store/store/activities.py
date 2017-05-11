import pytz
import time
import datetime
import dateutil.parser

from .google_calendar_backend import *
from .models import User, Activity


def sync_for_user(user):
    # Hardcode calendars for demo
    calendars = {
        "personal": "7eh6qnivid6430dl79ei89k26g@group.calendar.google.com",
        "exercise": "8puar0sc4e7efns5r849rn0lus@group.calendar.google.com",
        "medication": "us8v5j6ttp885542q9o2aljrho@group.calendar.google.com",
        "measurement": ""
    }

    # Get calendar service
    calendar_credentials = get_credentials()
    calendar_service = get_calendar_service(calendar_credentials)

    for calendar_type, calendar_id in calendars.iteritems():
        if not calendar_id:
            continue

        # Get the calendar
        calendar, res_code = get_calendar(calendar_service, calendar_id)

        # Calculate timeframe for fetching events
        tz = pytz.timezone(calendar['timeZone'])
        date_from = datetime.datetime.now(tz) - datetime.timedelta(days=7)
        date_to = datetime.datetime.now(tz) + datetime.timedelta(days=7)

        # Get events for the current calendar
        events = list_activities(calendar_service, calendar_id, date_from, date_to)

        # Add the activity type to the calendar object
        calendar['activity_type'] = calendar_type

        # Process the collected events
        process_events(user, calendar, events, date_from, date_to)

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

            # If the event differs from the already stored activity
            # then add the DB id to data so the entry is updated
            if event_equals_activity(event, db_event) != True:
                activity_data['id'] = db_event['id']

            # Remove the DB event from the hash because it is valid
            db_events_hash.pop(event['id'], None)

        Activity(**activity_data).save()

    # Delete the cancelled DB events (the ones remaining in the hash)
    activities_to_delete = map(
        lambda x: x['id'],
        db_events_hash.values()
    )
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

    if 'recurringEventId' in event:
        activity_data['recurring_event_id'] = event['recurringEventId']

    return activity_data

def event_equals_activity(event, activity):
    return False

def timestamp_from_event_date(date):
    return time.mktime(
        dateutil.parser.parse(date).timetuple()
    )


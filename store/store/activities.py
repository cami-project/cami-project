import pytz
import time
import datetime
import dateutil.parser

from .gcal_activity_backend import *
from .models import User, Activity


def sync_for_user(user):
    user = User.objects.get(username="camidemo")

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

        # Compose the color object
        calendar_colors = {
            'foreground': calendar['foregroundColor'],
            'background': calendar['backgroundColor'],
            'id': calendar['colorId']
        }

        # Get events for the current calendar
        events = list_activities(calendar_service, calendar_id, date_from, date_to)

        # Process the events
        for event in events:
            start = dateutil.parser.parse(event['start']['dateTime'])
            end = dateutil.parser.parse(event['end']['dateTime'])
            created = dateutil.parser.parse(event['created'])
            updated = dateutil.parser.parse(event['updated'])

            description = ""
            if 'description' in event:
                description = event['description']

            recurring_event_id = ""
            if 'recurringEventId' in event:
                recurring_event_id = event['recurringEventId']

            activity = Activity(
                event_id = event['id'],
                user = user,
                status = event['status'],
                html_link = event['htmlLink'],
                title = event['summary'],
                description = description,
                calendar_id = calendar_id,
                calendar_name = calendar['summary'],
                start = time.mktime(start.timetuple()),
                end = time.mktime(end.timetuple()),
                created = time.mktime(created.timetuple()),
                updated = time.mktime(updated.timetuple()),
                recurring_event_id = recurring_event_id,
                iCalUID = event['iCalUID'],
                reminders = event['reminders'],
                activity_type = calendar_type,
                color = calendar_colors,
                creator = event['creator']
            ).save()

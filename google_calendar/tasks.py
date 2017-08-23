import json
import time
import logging
import datetime

from kombu import Producer, Exchange, Connection
from celery import Celery
from logging import config

import settings
import activities
import store_utils


logging.config.dictConfig(settings.LOGGING)
logger = logging.getLogger("google_calendar")

app = Celery('tasks', broker=settings.BROKER_URL)
app.config_from_object(settings)

@app.task(name='google_calendar.sync_activities')
def sync_activities():
    logger.debug("[google_calendar] Synchronizing all users activities with Google Calendar. Number of users: %d" % 1)

    # Hardcode user for demo
    user = store_utils.user_get(2)
    app.send_task('google_calendar.sync_activities_for_user', [user])

@app.task(name='google_calendar.sync_activities_for_user')
def sync_activities_for_user(user):
    activities.sync_for_user(user)

@app.task(name='google_calendar.process_reminders')
def process_reminders():
    # Get current minute in UTC timestamp
    time_now = datetime.datetime.utcnow().strftime("%a, %d %b %Y %H:%M")
    time_now = time.mktime(time.strptime(time_now, "%a, %d %b %Y %H:%M"))

    # Get all the activities that should be reminded in the current minute
    # and send reminders for each one of them
    activities = store_utils.activity_get(reminders__contains=int(time_now))

    logger.debug(
        "[google_calendar] Processing reminders scheduled for %s. Number of reminders to be sent: %d",
        datetime.datetime.fromtimestamp(time_now).strftime('%H:%M'),
        len(activities)
    )

    for activity in activities:
        app.send_task('google_calendar.send_reminder', [activity, time_now])

@app.task(name='google_calendar.send_reminder')
def send_reminder(activity, timestamp):
    activity_type = activity['activity_type']
    caregiver_message_format = "Jim has %s at %s"

    if activity_type == 'personal':
        journal_entry_type = 'appointment'
        caregiver_message_format = caregiver_message_format % (
            "an appointment",
            "%s"
        )
    elif activity_type == 'exercise':
        journal_entry_type = 'exercise'
        caregiver_message_format = caregiver_message_format % (
            "to exercise",
            "%s"
        )
    elif activity_type == 'medication':
        journal_entry_type = 'medication'
        caregiver_message_format = caregiver_message_format % (
            "to take his medicine",
            "%s"
        )
    else:
        return

    activity_start = datetime.datetime.fromtimestamp(activity['start']).strftime('%H:%M')
    elder_message = activity['title'] + " at " + activity_start
    caregiver_message = caregiver_message_format % activity_start
    caregiver_description = activity['title'] + '\n' + activity['description']

    # Caregiver Journal Entry
    store_utils.insert_journal_entry(
        user="/api/v1/user/%d/" % 3,
        type=journal_entry_type,
        severity='none',
        timestamp=timestamp,
        message=caregiver_message,
        description=caregiver_description
    )

    # Elder Journal Entry
    store_utils.insert_journal_entry(
        user="/api/v1/user/%d/" % 2,
        type=journal_entry_type,
        severity='none',
        timestamp=timestamp,
        message=elder_message,
        description=activity['description']
    )

    with Connection(settings.BROKER_URL) as conn:
        channel = conn.channel()

        # Elder Push Notification
        payload = {
            "user_id": 2,
            "message": elder_message
        }
        inserter = Producer(
            exchange=Exchange('push_notifications', type='topic'),
            channel=channel,
            routing_key="push_notification"
        )
        inserter.publish(json.dumps(payload))

        # reminder_sent event
        payload = {
            "category": "user_notifications",
            "content": {
                "name": "reminder_sent",
                "value_type": "complex",
                "value": {
                    "user": { "id": 2 },
                    "activity": activity
                },
                "annotations": {
                    "timestamp": timestamp,
                    "source": "google_calendar"
                }
            }
        }
        inserter = Producer(
            exchange=Exchange('events', type='topic'),
            channel=channel,
            routing_key="event.user_notifications"
        )
        inserter.publish(json.dumps(payload))

    logger.debug(
        "[google_calendar] Successfully sent reminder for activity (%s).",
        str(activity)
    )

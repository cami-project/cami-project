import time
import logging
import datetime

from kombu import Queue, Exchange
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
    for activity in activities:
        app.send_task('google_calendar.send_reminder', [activity])

@app.task(name='google_calendar.send_reminder')
def send_reminder(activity):
    pass

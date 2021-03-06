# -*- coding: utf-8 -*-
import json
import time
import logging
import datetime
import requests
import uuid
import pytz

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

"""
================ Auxiliary functions ================
"""
translation_dict = {
    "enduser" : {
        "en" : "%s at %s",
        "ro" : "%s la ora %s",
        "dk" : "%s klokken %s",
        "pl" : "%s o %s"
    },
    "caregiver" : {
        "appointment" : {
            "en": "The person under your care has an appointment at %s",
            "ro": "Persoana in grija dvs. are o activitate la %s",
            "dk": "Personen du er plejer for har en aftale klokken %s",
            "pl": "Osoba, którą się opiekujesz ma wizytę o %s",
        },
        "exercise" : {
            "en": "The person under your care has an exercise at %s",
            "ro": "Persoana in grija dvs. are un exercitiu la %s",
            "dk": "Personen du er plejer for har en øvelse klokken %s",
            "pl": "Osoba, którą się opiekujesz ma ćwiczenia o %s",
        },
        "medication" : {
            "en": "The person under your care has to take their medicine at %s",
            "ro": "Persoana in grija dvs. trebuie sa-si ia medicamentele la %s",
            "dk": "Personen du er plejer for har skal tage sin medicin klokken %s",
            "pl": "Osoba, którą się opiekujesz ma wsiąć lekarstwa o %s",
        }
    }
}

timezone_dict = {
    "en": "Europe/London",
    "ro": "Europe/Bucharest",
    "pl": "Europe/Warsaw",
    "dk": "Europe/Copenhagen"
}

def _get_id_from_uri_path(uriPath):
    res = uriPath.rstrip("/")
    return int(res.split("/")[-1])

def _get_user_data(userURIPath):
    endpoint = settings.STORE_ENDPOINT_URI + userURIPath
    r = requests.get(endpoint)

    response_json = r.json()
    return r.status_code, response_json


@app.task(name='google_calendar.sync_activities')
def sync_activities():
    logger.debug("[google_calendar] Synchronizing all users activities with created Google Calendar instances. Number of calendars: %d" % 4)

    # Hardcode calendar for demo
    #user = store_utils.user_get_by_id(settings.CAMI_DEMO_USER_ID)
    app.send_task('google_calendar.sync_activities_for_calendar', [settings.CAMI_DEMO_CAL])

    # Hardcode user for DK user
    #user = store_utils.user_get_by_id(settings.TRIAL_USER_DK_ID)
    app.send_task('google_calendar.sync_activities_for_calendar', [settings.CAMI_DK_CAL])
    app.send_task('google_calendar.sync_activities_for_calendar', [settings.CAMI_DK_CAL_2])
    app.send_task('google_calendar.sync_activities_for_calendar', [settings.CAMI_DK_CAL_3])

    # Hardcode user for PL user
    #user = store_utils.user_get_by_id(settings.TRIAL_USER_PL_ID)
    app.send_task('google_calendar.sync_activities_for_calendar', [settings.CAMI_PL_CAL])

    # Hardcode user for RO user
    #user = store_utils.user_get_by_id(settings.TRIAL_USER_RO_ID)
    app.send_task('google_calendar.sync_activities_for_calendar', [settings.CAMI_RO_CAL])


@app.task(name='google_calendar.sync_activities_for_calendar')
def sync_activities_for_calendar(calendar_name):
    activities.sync_for_calendar(calendar_name)

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
    activity_user = activity['user']

    logger.debug(
        "[google_calendar] Preparing to send reminder for user %s. ",
        activity_user
    )

    call_status, user_data = _get_user_data(activity_user)
    if call_status != 200:
        logger.debug(
            "[google_calendar] Error. Call to user endpoint for %s failed. Reason: %s" % activity_user, user_data
        )

        return

    user_lang = user_data["enduser_profile"]["language"]
    user_id = int(user_data["id"])
    user_timezone_name = timezone_dict[user_lang]
    user_timezone = pytz.timezone(user_timezone_name)

    enduser_message_format = translation_dict["enduser"][user_lang]
    caregiver_message_format = "The person under your care has an activity at %s"

    if activity_type == 'personal':
        journal_entry_type = 'appointment'
        caregiver_message_format = translation_dict["caregiver"]["appointment"][user_lang]
        # caregiver_message_format = caregiver_message_format % (
        #     "an appointment",
        #     "%s"
        # )
    elif activity_type == 'exercise':
        journal_entry_type = 'exercise'
        caregiver_message_format = translation_dict["caregiver"]["exercise"][user_lang]
        # caregiver_message_format = caregiver_message_format % (
        #     "to exercise",
        #     "%s"
        # )
    elif activity_type == 'medication':
        journal_entry_type = 'medication'
        caregiver_message_format = translation_dict["caregiver"]["medication"][user_lang]
        # caregiver_message_format = caregiver_message_format % (
        #     "to take his medicine",
        #     "%s"
        # )
    else:
        caregiver_message_format = "The person under your care has an activity at %s"
        # return

    activity_start = datetime.datetime.fromtimestamp(activity['start'], tz = user_timezone).strftime('%H:%M')
    elder_message = enduser_message_format % (activity['title'], activity_start)
    caregiver_message = caregiver_message_format % activity_start
    caregiver_description = activity['title'] + '\n' + activity['description']

    logger.debug(
        "[google_calendar] Retrieved data for user that needs to be reminded: %s" % user_data
    )

    # Caregiver Journal Entry
    caregiver_journal_ids = []
    if "caregivers" in user_data["enduser_profile"]:
        for caregiver_uri in user_data["enduser_profile"]["caregivers"]:
            logger.debug(
                "[google_calendar] Creating journal entry for caregiver: %s",
                caregiver_uri
            )

            entry = store_utils.insert_journal_entry(
                user=caregiver_uri,
                type=journal_entry_type,
                severity='none',
                timestamp=timestamp,
                message=caregiver_message,
                description=caregiver_description
            )

            caregiver_journal_ids.append(int(entry["id"]))

    # Elder Journal Entry
    enduser_entry = store_utils.insert_journal_entry(
        user="/api/v1/user/%d/" % user_id,
        type=journal_entry_type,
        severity='none',
        timestamp=timestamp,
        message=elder_message,
        description=activity['description']
    )

    logger.debug(
        "[google_calendar] Journal entry created for enduser: %s" % enduser_entry
    )

    try:
        conn = Connection(settings.BROKER_URL)
        channel = conn.channel()

        # Elder Push Notification
        payload = {
            "user_id": user_id,
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
            "category": "USER_NOTIFICATIONS",
            "content": {
                "uuid": str(uuid.uuid4()),
                "name": "reminder_sent",
                "value_type": "complex",
                "value": {
                    "user": { "id": user_id },
                    "activity": activity,
                     "journal": {
                        "id_enduser": int(enduser_entry["id"]),
                        "id_caregivers": caregiver_journal_ids
                     },
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
    except Exception as ex:
        logger.error("[google_calendar] Error sending reminder push notification or event. Reason: %s" % ex)

    logger.debug(
        "[google_calendar] Successfully sent reminder for activity (%s)." % str(activity)
    )

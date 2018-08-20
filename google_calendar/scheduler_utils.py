import logging
import requests
from datetime import datetime

# Local imports
# import settings
from json_from_dict_conversion_methods import *

# logging.config.dictConfig(settings.LOGGING)
logger = logging.getLogger("google_calendar.scheduler_utils")

# must use CONTAINER_IP every time to connect to the Smart Scheduler Service
SCHEDULER_HOST = "172.18.0.2"
SCHEDULER_PORT = "8080"
SCHEDULER_ENDPOINT_URI = "http://" + SCHEDULER_HOST + ":" + SCHEDULER_PORT

# endpoint routes
API_ROUTE = "/api"
NEW_ACTIVITY_ROUTE = "/new_activity"
ACTIVITY_SCHEDULE_ROUTE = "/activity_schedule"
DELETE_ACTIVITY_ROUTE = "/delete_activity"

activity_type_to_category_converter = {"personal": "Leisure activities", "exercise": "Indoor physical exercises",
                                       "medication": "Medication intake",
                                       "health-measurement": "Imposed/Suggested Health measurements"}

activity_type_to_domain_converter = {"personal": "Leisure activities", "exercise": "Health Related Activities",
                                     "medication": "Health Related Activities",
                                     "health-measurement": "Health Related Activities"}


def get_datetime_from_timestamp(timestamp):
    return datetime.fromtimestamp(timestamp)


def get_duration_from_timestamps(start, end):
    return (end - start) / 60


# duration is in minutes
def add_duration_to_timestamp(timestamp_period, duration):
    return timestamp_period + duration * 60


def create_activity_dict_for_activity_from_calendar(activity_data):
    activity_datetime = get_datetime_from_timestamp(activity_data["start"])

    new_activities_dict = create_new_activities_dict(new_activities_list=
    [create_new_activity_dict(activity=
    create_activity_dict(
        activity_type_dict=create_activity_type_dict(code=activity_data["title"],
                                                     duration=get_duration_from_timestamps(
                                                         activity_data["start"],
                                                         activity_data["end"]),
                                                     activity_category=create_activity_category_dict(
                                                         code=activity_type_to_category_converter[
                                                             activity_data["activity_type"]],
                                                         domain=create_activity_domain_dict(
                                                             code=activity_type_to_domain_converter[
                                                                 activity_data["activity_type"]])),
                                                     description=activity_data["description"],
                                                     imposed_period=create_period_dict(
                                                         hour=activity_datetime.hour,
                                                         minutes=activity_datetime.minute,
                                                         day_index=activity_datetime.weekday())),
        uuid=activity_data["event_id"], immovable=False))
    ])

    return new_activities_dict


def create_deleted_activities(activity_data):
    deleted_activities_dict = create_deleted_activities_dict(
        [create_deleted_activity_dict(name=activity_data["title"], uuid=activity_data["event_id"])])
    return deleted_activities_dict


def activity_post(**kwargs):
    method = 'POST'
    endpoint = SCHEDULER_ENDPOINT_URI + API_ROUTE + NEW_ACTIVITY_ROUTE
    activity_data = dict(kwargs)

    # create the new_activities_dict using activity_data
    new_activities_dict = create_activity_dict_for_activity_from_calendar(activity_data)

    r = requests.request(method, endpoint, json=new_activities_dict)

    if r.ok:
        logger.debug("[google_calendar.scheduler_utils] Added a new activity %s" % new_activities_dict)
        return True

    logger.debug(
        "[google_calendar.scheduler_utils] " +
        "There was a problem adding the activities to Scheduler. " +
        "Arguments: %s. Response: %s" % (new_activities_dict, r.text)
    )

    return False


def activity_schedule_get():
    endpoint = SCHEDULER_ENDPOINT_URI + API_ROUTE + ACTIVITY_SCHEDULE_ROUTE

    r = requests.get(endpoint)

    activity_schedule = r.json()

    if r.ok:
        logger.debug("[google_calendar.scheduler_utils] Got the activities from Scheduler... ")
        return activity_schedule

    logger.debug(
        "[google_calendar.scheduler_utils] " +
        "There was a problem fetching activities from Scheduler. " +
        "Response: %s" % r.text
    )

    return False


def activity_delete(**kwargs):
    method = 'DELETE'
    endpoint = SCHEDULER_ENDPOINT_URI + API_ROUTE + DELETE_ACTIVITY_ROUTE
    activity_data = dict(kwargs)

    # create the deleted_activities_dict using activity_data
    deleted_activities_dict = create_deleted_activities(activity_data)

    r = requests.request(method, endpoint, json=deleted_activities_dict)

    if r.ok:
        logger.debug("[google_calendar.scheduler_utils] Deleted the activity %s " % deleted_activities_dict)
        return True

    logger.debug(
        "[google_calendar.scheduler_utils] " +
        "There was a problem deleting the activities from Scheduler. " +
        "Arguments: %s. Response: %s" % (deleted_activities_dict, r.text)
    )

    return False

# print activity_post()
# print activity_delete()
# print activity_schedule_get()
# print get_datetime_from_timestamp(1534431600)
# print get_datetime_from_timestamp(1534431600).weekday()
# print get_datetime_from_timestamp(1534431600).hour
# print get_datetime_from_timestamp(1534431600).minute
# print get_duration_from_timestamps(1534431600, 1534433400)

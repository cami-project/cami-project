import logging
import requests

# Local imports
import settings

logging.config.dictConfig(settings.LOGGING)
logger = logging.getLogger("scheduler_utils")

# must use CONTAINER_IP to connect to the scheduler service
SCHEDULER_HOST = "172.17.0.2"
SCHEDULER_PORT = "8080"
SCHEDULER_ENDPOINT_URI = "http://" + SCHEDULER_HOST + ":" + SCHEDULER_PORT

# endpoint routes
API_ROUTE = "/api"
NEW_ACTIVITY_ROUTE = "new_activity"
ACTIVITY_SCHEDULE_ROUTE = "/activity_schedule"
DELETE_ACTIVITY_ROUTE = "/delete_activity"


def activity_post(**kwargs):
    method = 'POST'
    endpoint = SCHEDULER_ENDPOINT_URI + API_ROUTE + NEW_ACTIVITY_ROUTE
    activity_data = dict(kwargs)

    # TODO
    # create the new_activities_dict using activity_data
    new_activities_dict = dict()

    r = requests.request(method, endpoint, params=new_activities_dict)

    if r == 200:
        return True

    return False


def activity_schedule_get():
    endpoint = SCHEDULER_ENDPOINT_URI + API_ROUTE + ACTIVITY_SCHEDULE_ROUTE

    r = requests.get(endpoint)

    activity_schedule = r.text

    if r.ok:
        return activity_schedule

    logger.debug(
        "[google_calendar.scheduler_utils] " +
        "There was a problem fetching activities from Smart Scheduler. " +
        "Response: %s" % r.text
    )

    return False


def activity_delete(**kwargs):
    endpoint = SCHEDULER_ENDPOINT_URI + API_ROUTE + DELETE_ACTIVITY_ROUTE
    activity_data = dict(kwargs)

    # TODO
    # create the deleted_activities_dict using activity_data
    deleted_activities_dict = dict()
    r = requests.delete(endpoint, params=deleted_activities_dict)

    if r == 200:
        return True

    return False

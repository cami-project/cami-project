import logging
import requests

# Local imports
import settings


logging.config.dictConfig(settings.LOGGING)
logger = logging.getLogger("google_calendar.store_utils")

def activity_get(**kwargs):
    endpoint = settings.STORE_ENDPOINT_URI + "/api/v1/activity/"
    r = requests.get(endpoint, params=dict(kwargs))

    json_response = r.json()
    if 'activities' in json_response:
        return json_response['activities']

    logger.debug(
        "[google_calendar_store_utils] " +
        "There was a problem fetching activities from Store. " +
        "Arguments: %s. Response: %s" % (kwargs, r.text)
    )
    return False

def activity_save(**kwargs):
    endpoint = settings.STORE_ENDPOINT_URI + "/api/v1/activity/"

    method = 'POST'
    data = dict(kwargs)
    if 'id' in data:
        endpoint = endpoint + str(data['id']) + "/"
        data.pop('id', None)
        method = 'PUT'
    
    r = requests.request(
        method,
        endpoint,
        json=data
    )

    if r.status_code in [200, 201]:
        return True

    logger.debug(
        "[google_calendar_store_utils] " +
        "There was a problem saving the activity to Store. " +
        "Arguments: %s. Response: %s" % (kwargs, r.text)
    )
    return False

def activity_delete(**kwargs):
    endpoint = settings.STORE_ENDPOINT_URI + "/api/v1/activity/"
    r = requests.delete(endpoint, params=dict(kwargs))

    if r.status_code == 204:
        return True

    logger.debug(
        "[google_calendar_store_utils] " +
        "There was a problem deleting the activities from Store. " +
        "Arguments: %s. Response: %s" % (kwargs, r.text)
    )
    return False

def user_get(id):
    endpoint = settings.STORE_ENDPOINT_URI + "/api/v1/user/" + str(id)
    r = requests.get(endpoint)

    json_response = r.json()
    if json_response:
        return json_response

    return False

def insert_journal_entry(**kwargs):
    endpoint = settings.STORE_ENDPOINT_URI + "/api/v1/journal_entries/"

    method = 'POST'
    data = dict(kwargs)

    r = requests.request(
        method,
        endpoint,
        json=data
    )

    if r.status_code in [200, 201]:
        return r.json()

    logger.debug(
        "[medical_compliance.store_utils] " +
        "Failed inserting a new Journal Entry. " +
        "Arguments: %s. Response: %s" % (kwargs, r.text)
    )
    return False

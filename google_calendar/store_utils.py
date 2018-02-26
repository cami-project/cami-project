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
        "[google_calendar.store_utils] " +
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
        "[google_calendar.store_utils] " +
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
        "[google_calendar.store_utils] " +
        "There was a problem deleting the activities from Store. " +
        "Arguments: %s. Response: %s" % (kwargs, r.text)
    )
    return False



def user_get_by_url(user_url):
    logger.debug(
        "[google_calendar.store_utils] Retrieving user details for user %s" % user_url)

    r = requests.get(user_url)

    json_response = r.json()
    if json_response:
        return json_response

    return False


def user_get_by_id(id):
    endpoint = settings.STORE_ENDPOINT_URI + "/api/v1/user/" + str(id)
    return user_get_by_url(endpoint)


def user_get_by_calendar(calendar_name):
    """
    Return the details for the user who is currently associated with the calendar given by `calendar_name`
    :param calendar_name: the calendar_name of the calendar
    :return: the details for user associated with the calendar
    """
    endpoint = settings.STORE_ENDPOINT_URI + "/api/v1/externalservice/"
    params = { "name": calendar_name, "order_by": "-updated_at"}

    r = requests.get(url=endpoint, params=params)

    if r.status_code == 200:
        json_response = r.json()
        if not json_response["objects"]:
            logger.debug(
                "[google_calendar.store_utils] No user association exists for the %s calendar. Reverting to default user" % calendar_name)

            return user_get_by_id(settings.TRIAL_USER_IDs[calendar_name])
        else:
            user_uri_path = json_response["objects"][0]["user"]
            user_url = settings.STORE_ENDPOINT_URI + user_uri_path

            return user_get_by_url(user_url)
    else:
        logger.debug(
            "[google_calendar.store_utils] " +
            "There was a problem determining the association of the %s calendar to a given user. "
            "Reverting to default user. \n Response was: %s" % (calendar_name, r.text)
        )

        return user_get_by_id(settings.TRIAL_USER_IDs[calendar_name])




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
        "[google_calendar.store_utils] " +
        "Failed inserting a new Journal Entry. " +
        "Arguments: %s. Response: %s" % (kwargs, r.text)
    )
    return False

import logging
import requests

# Local imports
import settings


logging.config.dictConfig(settings.LOGGING)
logger = logging.getLogger("frontend.store_utils")

def pushnotificationdevice_get(**kwargs):
    endpoint = settings.STORE_ENDPOINT_URI + "/api/v1/pushnotificationdevice/"
    r = requests.get(endpoint, params=dict(kwargs))

    json_response = r.json()
    if 'objects' in json_response:
        return json_response['objects']

    logger.debug(
        "[frontend_store_utils] " +
        "There was a problem fetching push notification devices from Store. " +
        "Arguments: %s. Response: %s" % (kwargs, r.text)
    )
    return False

def pushnotificationdevice_save(**kwargs):
    endpoint = settings.STORE_ENDPOINT_URI + "/api/v1/pushnotificationdevice/"

    method = 'POST'
    data = dict(kwargs)
    
    if 'registration_id' in kwargs:
        notification_devices = pushnotificationdevice_get(
            registration_id=kwargs['registration_id']
        )
        if notification_devices and len(notification_devices) == 1:
            endpoint = endpoint + str(notification_devices[0]['id']) + "/"
            method = 'PUT'
    
    r = requests.request(
        method,
        endpoint,
        json=data
    )

    if r.status_code in [200, 201]:
        return r.json()

    logger.debug(
        "[frontend_store_utils] " +
        "There was a problem saving the push notification device to Store. " +
        "Arguments: %s. Response: %s" % (kwargs, r.text)
    )
    return False

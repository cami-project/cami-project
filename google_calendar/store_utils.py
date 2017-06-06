import logging
import requests

from collections import namedtuple

# Local imports
import settings


logging.config.dictConfig(settings.LOGGING)
logger = logging.getLogger("google_calendar.store_utils")

def activity_get(**kwargs):
    endpoint = settings.STORE_ENDPOINT_URI + "/api/v1/activity/"
    r = requests.get(endpoint, params=dict(kwargs))
    return r.json()['activities']

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

    return r.status_code == 204

def activity_delete(**kwargs):
    endpoint = settings.STORE_ENDPOINT_URI + "/api/v1/activity/"
    r = requests.delete(endpoint, params=dict(kwargs))
    return r.status_code == 204

def user_get(id):
    endpoint = settings.STORE_ENDPOINT_URI + "/api/v1/user/" + str(id)
    r = requests.get(endpoint)
    return r.json()

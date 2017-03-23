import requests
import logging

STORE_HOST = "cami-store-management"
STORE_PORT = "8008"
STORE_ENDPOINT_URI = "http://" + STORE_HOST + ":" + STORE_PORT

logger = logging.getLogger("medical_compliance.store_utils")

def get_id_from_uri_path(uri_path):
    """
    Retrieves the numeric id from an uri_path that identifies an individual resource (e.g. /api/v1/device/1/, /api/v1/user/14/)
    :param uri_path: the uri path of an individual resource
    :return: the id of the resource
    """
    return int(uri_path.strip("/").split("/")[-1])

def make_user_uri_path(user_id):
    return "/api/v1/user/" + str(user_id) + "/"

def make_device_uri_path(device_id):
    return "/api/v1/device/" + str(device_id) + "/"



def get_device(store_endpoint_host_uri, **kwargs):
    uri_path = "/api/v1/device/"

    uri = store_endpoint_host_uri + uri_path
    query_params = {k:v for k, v in kwargs.items() if k in ['id', 'manufacturer', 'model', 'serial_number']}

    r =  requests.get(uri, params=query_params)
    response_json = r.json()

    ## we raise an exception if the response returns more than one results
    if len(response_json['devices']) > 1:
        raise ValueError("Query for single device at %s with query_params %s returned more than one result."
                         % (uri, str(kwargs)))

    if response_json['devices']:
        device_data = response_json['devices'][0]
        return device_data

    return None


def get_device_usage(store_endpoint_host_uri, **kwargs):
    uri_path = "/api/v1/deviceusage/"

    uri = store_endpoint_host_uri + uri_path
    query_params = {k: v for k, v in kwargs.items() if k in ['user', 'device']}

    if "access_info" in kwargs:
        if isinstance(kwargs['access_info'], dict):
            for key in kwargs['access_info']:
                query_key = "access_info" + "__" + key
                query_params[query_key] = kwargs['access_info'][key]
        else:
            raise ValueError("Supplied value for DeviceUsage access_info field must be a Python dict. Supplied type: %s." % type(kwargs['access_info']))

    r = requests.get(uri, params=query_params)
    response_json = r.json()

    ## we raise an exception if the response returns more than one results
    if len(response_json['objects']) > 1:
        raise ValueError("Query for single DeviceUsage pair at %s with query_params %s returned more than one result."
                         % (uri, str(kwargs)))

    if response_json['objects']:
        device_usage_data = response_json['objects'][0]

        access_info = device_usage_data['access_info']
        device_id = get_id_from_uri_path(device_usage_data['device'])
        user_id = get_id_from_uri_path(device_usage_data['user'])

        return {
            "user_id" : user_id,
            "device_id": device_id,
            "access_info": access_info
        }

    return None


def get_measurements(store_endpoint_host_uri, **kwargs):
    uri_path = "/api/v1/measurement/"
    uri = store_endpoint_host_uri + uri_path

    query_params = {k: v for k, v in kwargs.items()}

    r = requests.get(uri, params=query_params)
    response_json = r.json()

    if response_json['measurements']:
        return response_json['measurements']

    return None


def insert_measurement(store_endpoint_host_uri, user_id, device_id,
                       measurement_type, measurement_unit,
                       timestamp, timezone,
                       value_info, context_info = None):

    uri_path = "/api/v1/measurement/"
    uri = store_endpoint_host_uri + uri_path

    user_uri = make_user_uri_path(user_id)
    device_uri = make_device_uri_path(device_id)

    payload = {
        "measurement_type": measurement_type,
        "unit_type": measurement_unit,

        "timestamp": timestamp,
        "timezone": timezone,

        "user": user_uri,
        "device": device_uri,
        "value_info": value_info
    }

    if context_info and isinstance(context_info, dict):
        payload['context_info'] = context_info

    r = requests.post(uri, json=payload)

    if r.status_code == 201:
        return r.status_code, r.json()
    else:
        logger.error("[medical_compliance.store_utils] Failed to insert measurement regarding "
                     "user_id=%s, device_id=%s, measurement_type=%s, measurement_unit=%s, "
                     "timestamp=%s, timezone=%s, value_info=%s, context_info=%s."
                     % (user_id, device_id, measurement_type, measurement_unit, str(timestamp), timezone, str(value_info), str(context_info)))
        r.raise_for_status()


import requests
import urlparse
import json
import datetime, pytz
import constants

from custom_logging import logger
from settings import LINKWATCH_URL_BASE, LINKWATCH_USER, LINKWATCH_PASSWORD

class EndpointResult(object):
    '''
    Base wrapper over the returned `request' response content
    '''
    def __init__(self, response=None):
        self.response = response

    def is_error(self):
        '''
        :return: True if the status code of the HTTP Response is in the client or server error range (i.e. >= 400)
        '''
        if self.response.status_code >= 400:
            return True

        return False

    def get_error_reason(self):
        if self.is_error():
            return self.response.reason
        else:
            return "No error."

    def get_status(self):
        return self.response.status_code

    def get_text(self):
        return self.response.text


class LoginResult(EndpointResult):
    def get_token(self):
        '''
        :return: Returns the API token used in subsequent calls, retrieving it from the returned
        '''
        if self.is_error():
            return None
        else:
            return self.response.headers['Token']


class ObservationResult(EndpointResult):

    def get_measurement(self):
        pass

    def created_ok(self):
        pass


class Endpoint(object):
    def __init__(self, url, headers = None, data = None, return_cls = EndpointResult):
        self.url = url
        self.data = data
        self.return_cls = return_cls

        self.headers = {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
        if headers:
            self.headers.update(headers)


class LoginEndpoint(Endpoint):
    def __init__(self, username, password):
        data = {
            u"UserName": username,
            u"Password": password
        }
        url = urlparse.urljoin(LINKWATCH_URL_BASE, "api/v1/authentication/login")

        super(LoginEndpoint, self).__init__(url, data = data, return_cls=LoginResult)

    def post(self):
        response = requests.post(self.url, headers = self.headers, data = json.dumps(self.data))
        return self.return_cls(response)


class SendObservationsEndpoint(Endpoint):
    def __init__(self, token, observations):
        url = urlparse.urljoin(LINKWATCH_URL_BASE, "api/v1/observation")
        headers = self._set_headers(token)
        data = self._set_data(observations)

        super(SendObservationsEndpoint, self).__init__(url, headers=headers, data=data, return_cls=ObservationResult)

    def post(self):
        response = requests.post(self.url, headers = self.headers, data = json.dumps(self.data))
        return self.return_cls(response)

    def _set_headers(self, token):
        return {"Token" : token}

    def _set_data(self, observations):
        if observations:
            data = []
            for observation in observations:
                obs_data = {}
                obs_data["TypeId"] = observation.device_type
                obs_data["Timestamp"] = observation.timestamp.strftime("%Y-%m-%dT%H:%M:%S%z")
                obs_data["InputType"] = observation.input_type
                obs_data["Comment"] = "Test measurement"

                measurements = []
                contexts = []
                for m in observation.measurements:
                    measurements.append(m.get_measurement_payload())
                    contexts.append(m.get_context_payload())

                obs_data["Measurements"] = measurements
                obs_data["Contexts"] = contexts

                data.append(obs_data)

            return data

        return None


class GetObservationsEndpoint(Endpoint):
    pass
    # def get(self):
    #     '''
    #     Run a GET request to the url of the endpoint.
    #     The `data' parameter is used to form the query string of the GET request.
    #     :return: an EndpointResult object
    #     '''
    #     response = requests.get(self.url, headers = self.headers, params = self.data)
    #     return self.return_cls(response)


class Measurement(object):
    def __init__(self, type, value, measurement_unit, context_type = None):
        '''
        :param type: Type of measurement, itneger value of one of the constants from :class:`constants.Measurement`
        :param value: Value of measurement given as string
        :param measurement_unit: Type of measurement unit, integer value of one of the constants from :class:`constants.UnitCode`
        :param context_type: Type of context for the measurement, given as a tuple (type_id, description). May be None.
        '''
        self.type = type
        self.value = value
        self.measurement_unit = measurement_unit
        self.context_type = context_type

    def get_measurement_payload(self):
        return {
            "TypeId" : self.type,
            "Value": str(self.value),
            "UnitCode": self.measurement_unit
        }

    def get_context_payload(self):
        if self.context_type:
            return \
                {
                    "TypeId" : self.context_type[0],
                    "Context": self.context_type[1]
                }
        else:
            return \
                {
                    "TypeId": None,
                    "Context": None
                }

    def __str__(self):
        return "{ type: %s, value: %s, measurement_unit: %s, context_type: %s }" % (self.type, self.value, self.measurement_unit, self.context_type)

    __unicode__ = __str__


class Observation(object):
    def __init__(self, device_type, timestamp, input_type, equipment_id = None, measurements = None, comment = None):
        self.device_type = device_type
        self.timestamp = timestamp
        self.input_type = input_type
        self.equipment_id = equipment_id
        self.measurements = measurements
        self.comment = comment

    def __str__(self):
        return "{ device_type: %s, timestamp: %s, input_type: %s, equipment_id: %s, measurements: %s, comment: %s}" % \
            (self.device_type, self.timestamp, self.input_type, self.equipment_id, str(self.measurements), self.comment)

def process_measurement(measurement_json):
    login_res = perform_login()
    token = login_res.get_token()

    if not token:
        logger.error("[linkwatch] Auth token unavailable due to error in login_endpoint call. Reason: %s" % (login_res.get_error_reason()))
    else:
        logger.info("[linkwatch] Auth token value is: %s" % (token))

    obs = None
    try:
        obs = get_observation_from_measurement_json(measurement_json)
    except Exception as e:
        logger.error("[linkwatch] Could not convert measurement -- %s -- to linkwatch observation: %s" % (measurement_json, e))
        return

    send_observation(token, obs)

def perform_login():
    logger.debug("[linkwatch] Getting auth token...")

    login_endpoint = LoginEndpoint(LINKWATCH_USER, LINKWATCH_PASSWORD)
    login_res = login_endpoint.post()
    if login_res.is_error():
        logger.error("[linkwatch] Could not log demo user in. Login result: %s" % (login_res.get_error_reason()))

    return login_res

def get_observation_from_measurement_json(measurement_json):
    logger.debug("[linkwatch] Converting measurement %s to linkwatch observation..." % (measurement_json))

    #t = datetime.datetime.fromtimestamp(measurement_json['timestamp'])
    t = datetime.datetime.utcfromtimestamp(measurement_json['timestamp'])

    if measurement_json['type'] == 'weight':
        weight_measurement = Measurement(constants.MeasurementType.WEIGHT, measurement_json['value'], constants.UnitCode.KILOGRAM)
        return Observation(constants.DeviceType.WEIGHTING_SCALE, t, "TEST", measurements=[weight_measurement], comment=measurement_json['input_source'])
    elif measurement_json['type'] == 'heartrate':
        heartrate_measurement = Measurement(constants.MeasurementType.PULSE_RATE_NON_INV, measurement_json['value'], constants.UnitCode.BPM)
        return Observation(constants.DeviceType.BLOODPRESSURE, t, "TEST", measurements=[heartrate_measurement], comment=measurement_json['input_source'])
    elif measurement_json['type'] == 'steps':
        steps_measurement = Measurement(constants.MeasurementType.HF_STEPS, measurement_json['value'], constants.UnitCode.STEP)
        return Observation(constants.DeviceType.STEP_COUNTER, t, "TEST", measurements=[steps_measurement], comment=measurement_json['input_source'])

    raise Exception("Unsupported measurement type: %s" % (measurement_json['type']))


def send_observation(api_token, observation):
    obs_endpoint = SendObservationsEndpoint(api_token, [observation])
    obs_res = obs_endpoint.post()

    if not obs_res.is_error():
        logger.info("[linkwatch] Observation POST result for %s: %s -- with HTTP STATUS: %s" % (observation, obs_res.get_text(), obs_res.get_status()))
    else:
        try:
            obs_res.response.raise_for_status()
        except Exception, e:
            logger.error("[linkwatch] Failed to POST new weight observation %s: %s" % (observation, e))

if __name__ == "__main__":
    process_measurement({
        'type': 'weight',
        'user_id': 1234,
        'input_source': 'withings',
        'timestamp': 1474965274,
        'value': 79.1,
        'timezone': 'Europe/Bucharest'
    })
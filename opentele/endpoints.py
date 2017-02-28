import requests
from requests.auth import HTTPBasicAuth

import json, pprint
import logging
import datetime, pytz
import constants

from settings import OPENTELE_URL_BASE, OPENTELE_USER, OPENTELE_PASSWORD

from custom_logging import logger

class GetObservations(object):
    def __init__(self, url, credentials, params = None):
        self.url = url
        self.credentials = credentials
        self.params = params


    def get(self):
        '''
        Run a GET request to the url of the corresponding measurement endpoint.
        The `params' parameter is used to form the query string of the GET request.
        :return: a tuple of the HTTP return code and the JSON string in case of a successful request, or None otherwise
        '''
        response = requests.get(self.url, auth=HTTPBasicAuth(self.credentials['user'], self.credentials['pass']), params = self.params)
        if response.status_code >= 200 and response.status_code < 300:
            return (response.status_code, json.dumps(json.loads(response.text), indent=4))
        else:
            return (response.status_code, None)


class GetWeightObservations(GetObservations):
    URL = OPENTELE_URL_BASE + "/patient/measurements/weight"

    def __init__(self, credentials, params):
        super(GetWeightObservations, self).__init__(GetWeightObservations.URL, credentials, params)


class GetBPObservations(GetObservations):
    URL = OPENTELE_URL_BASE + "/patient/measurements/blood_pressure"

    def __init__(self, credentials, params):
        super(GetBPObservations, self).__init__(GetBPObservations.URL, credentials, params)


class GetSaturationObservations(GetObservations):
    URL = OPENTELE_URL_BASE + "/patient/measurements/saturation"

    def __init__(self, credentials, params):
        super(GetSaturationObservations, self).__init__(GetSaturationObservations.URL, credentials, params)



class SendObservation(object):
    URL = OPENTELE_URL_BASE + "/rest/questionnaire/listing/"

    def __init__(self, credentials, observation, url = None):
        if url:
            self.url = url
        else:
            self.url = SendObservation.URL

        self.credentials = credentials
        self.observation = observation

    def post(self):

        logger.debug("[opentele] Attempting to post to OpenTele: %s" % (json.dumps(self.observation.get_payload())))

        response = requests.post(self.url,
                                 auth=HTTPBasicAuth(self.credentials['user'], self.credentials['pass']),
                                 data=json.dumps(self.observation.get_payload()))
        return response


class SendBP(SendObservation):
    def __init__(self, credentials, systolic, diastolic, pulse, timestamp):

        obs = self._prepare_bp_observation(systolic, diastolic, pulse, timestamp)
        super(SendBP, self).__init__(credentials, obs)

    def _prepare_bp_observation(self, systolic, diastolic, pulse, timestamp):
        measurements = []

        if systolic:
            measurements.append(Measurement(constants.MeasurementType.CAMI_BP + "#SYSTOLIC", "Int", systolic))

        if diastolic:
            measurements.append(Measurement(constants.MeasurementType.CAMI_BP + "#DIASTOLIC", "Int", diastolic))

        if pulse:
            measurements.append(Measurement(constants.MeasurementType.CAMI_BP + "#PULSE", "Int", pulse))

        measurements.append(Measurement(constants.MeasurementType.CAMI_BP + "##SEVERITY", "String", "GREEN"))

        return Observation(constants.ObservationName.CAMI_BLOOD_PRESSURE, timestamp, constants.QuestionnaireID.CAMI_BP,
                           measurements = measurements)



class SendWeight(SendObservation):
    def __init__(self, credentials, weight, timestamp):
        obs = self._prepare_weight_observation(weight, timestamp)
        super(SendWeight, self).__init__(credentials, obs)

    def _prepare_weight_observation(self, weight, timestamp):
        weight_meas = Measurement(constants.MeasurementType.CAMI_WEIGHT, "Float", weight)
        severity_meas = Measurement(constants.MeasurementType.CAMI_WEIGHT + "#SEVERITY", "String", "GREEN")

        return Observation(constants.ObservationName.CAMI_WEIGHT, timestamp, constants.QuestionnaireID.CAMI_WEIGHT,
                           measurements=[weight_meas, severity_meas])



class Measurement(object):
    def __init__(self, name, type, value):
        '''
        :param name: name of the measurement as set by the OpenTele API, given as String
        :param type: Type of the measurement value (e.g. String, Float, Integer), given as string
        :param value: Value of measurement
        '''
        self.name = name
        self.type = type
        self.value = value


    def get_payload(self):
        return {
            "name" : self.name,
            "type": self.type,
            "value": self.value
        }



class Observation(object):
    def __init__(self, name, timestamp, questionnaire_id, measurements = None):
        self.name = name
        self.timestamp = datetime.datetime.fromtimestamp(timestamp)
        self.questionnaire_id = questionnaire_id
        self.measurements = measurements

    def get_payload(self):
        if self.measurements:
            return {
                "name": self.name,
                "date": self.timestamp.strftime("%Y-%m-%dT%H:%M:%S%z"),
                "QuestionnaireId": self.questionnaire_id,
                "version": "1.0",
                "output": [m.get_payload() for m in self.measurements]
            }
        else:
            return None

def get_credentials():
    return {
        'user': OPENTELE_USER,
        'pass': OPENTELE_PASSWORD
    }

def process_measurement(measurement_json):
    logger.debug("[opentele] Processing %s measurement: %s" % (measurement_json['type'], measurement_json))

    if measurement_json['type'] == 'weight':
        send_weight_req = SendWeight(get_credentials(), measurement_json['value'], measurement_json['timestamp'])
        res = send_weight_req.post()

        if (res.status_code < 400 ):
            logger.debug("[opentele] The result of posting %s to OpenTele: %s -- with HTTP STATUS: %s" % (measurement_json, str(res.text), str(res.status_code)))
        else:
            logger.error("[opentele] The result of posting %s to OpenTele: %s -- with HTTP STATUS: %s" % (measurement_json, str(res.text), str(res.status_code)))

        return

    elif measurement_json['type'] == 'heartrate':
        send_bp_req = SendBP(get_credentials(), systolic=None, diastolic=None, pulse=measurement_json['value'],
                                timestamp = measurement_json['timestamp'])
        res = send_bp_req.post()

        if (res.status_code < 400 ):
            logger.debug("[opentele] The result of posting %s to OpenTele: %s -- with HTTP STATUS: %s" % (measurement_json, str(res.text), str(res.status_code)))
        else:
            logger.error("[opentele] The result of posting %s to OpenTele: %s -- with HTTP STATUS: %s" % (measurement_json, str(res.text), str(res.status_code)))

        return

    raise Exception("Unsupported measurement type: %s" % (measurement_json['type']))

if __name__ == "__main__":
    # Basic HTTP AUTH data
    credentials = get_credentials()

    # ==== Send a weight measurement ====
    logger.info("Sending a weight measurement ...")
    send_weight = SendWeight(credentials, 70.2, 1485703734)
    res = send_weight.post()
    logger.info("Status code: " + str(res))

    send_weight = SendWeight(credentials, 70.2, 1485703734)
    res = send_weight.post()
    logger.info("Status code: " + str(res))

    # check that it has been registered
    get_weight = GetWeightObservations(credentials, params={"filter": "week"})
    status, json_str = get_weight.get()
    if status == 200:
        print json_str

    # ==== Send a BP measurement ====
    print("Sending a bp measurement ...")
    send_bp1 = SendBP(credentials, systolic=None, diastolic=None, pulse=75, timestamp=1485703734)
    send_bp2 = SendBP(credentials, systolic=None, diastolic=None, pulse=76, timestamp=1485703734)
    res1 = send_bp1.post()
    res2 = send_bp2.post()
    print("Status code1: " + str(res1))
    print("Status code2: " + str(res2))

    # check that it has been registered
    get_bp = GetBPObservations(credentials, params={"filter": "week"})
    status, json_str = get_bp.get()
    if status == 200:
        print json_str

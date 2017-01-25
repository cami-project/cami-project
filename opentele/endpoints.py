import requests
from requests.auth import HTTPBasicAuth

import json, pprint
import logging
import datetime, pytz
import constants

from settings import OPENTELE_URL_BASE, OPENTELE_USER, OPENTELE_PASSWORD

logging.basicConfig()
logger = logging.getLogger(name="endpoints")
logger.setLevel(logging.INFO)


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
        response = requests.post(self.url,
                                 auth=HTTPBasicAuth(self.credentials['user'], self.credentials['pass']),
                                 data=json.dumps(self.observation.get_payload()))
        return response.status_code


class SendBP(SendObservation):
    def __init__(self, credentials, systolic, diastolic, pulse):

        obs = self._prepare_bp_observation(systolic, diastolic, pulse)
        super(SendBP, self).__init__(credentials, obs)

    def _prepare_bp_observation(self, systolic, diastolic, pulse):
        systolic_meas = Measurement(constants.MeasurementType.CAMI_BP + "#SYSTOLIC", "Int", systolic)
        diastolic_meas = Measurement(constants.MeasurementType.CAMI_BP + "#DIASTOLIC", "Int", diastolic)
        pulse_meas = Measurement(constants.MeasurementType.CAMI_BP + "#PULSE", "Int", pulse)
        severity_meas = Measurement(constants.MeasurementType.CAMI_BP + "##SEVERITY", "String", "GREEN")

        ts = datetime.datetime.now(tz = pytz.timezone("Europe/Bucharest"))

        return Observation("CAMIBloodPresure", ts, constants.QuestionnaireID.CAMI_BP,
                           measurements = [systolic_meas, diastolic_meas, pulse_meas, severity_meas])



class SendWeight(SendObservation):
    def __init__(self, credentials, weight):
        obs = self._prepare_weight_observation(weight)
        super(SendWeight, self).__init__(credentials, obs)

    def _prepare_weight_observation(self, weight):
        weight_meas = Measurement(constants.MeasurementType.CAMI_WEIGHT, "Float", weight)
        severity_meas = Measurement(constants.MeasurementType.CAMI_WEIGHT + "#SEVERITY", "String", "GREEN")

        ts = datetime.datetime.now(tz=pytz.timezone("Europe/Bucharest"))

        return Observation("CAMIBloodPresure", ts, constants.QuestionnaireID.CAMI_WEIGHT,
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
        self.timestamp = timestamp
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


if __name__ == "__main__":
    # Basic HTTP AUTH data
    credentials = {
        'user': OPENTELE_USER,
        'pass': OPENTELE_PASSWORD
    }


    # ==== Send a weight measurement ====
    logger.info("Sending a weight measurement ...")
    send_weight = SendWeight(credentials, 70.2)
    res = send_weight.post()
    logger.info("Status code: " + str(res))

    # check that it has been registered
    get_weight = GetWeightObservations(credentials, params={"filter": "week"})
    status, json_str = get_weight.get()
    if status == 200:
        print json_str

    # ==== Send a BP measurement ====
    logger.info("Sending a weight measurement ...")
    send_bp = SendBP(credentials, systolic=115, diastolic=60, pulse=75)
    res = send_bp.post()
    logger.info("Status code: " + str(res))

    # check that it has been registered
    get_bp = GetBPObservations(credentials, params={"filter": "week"})
    status, json_str = get_bp.get()
    if status == 200:
        print json_str

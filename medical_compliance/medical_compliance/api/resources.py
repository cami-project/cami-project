from django.conf.urls import url
from tastypie.authentication import Authentication
from tastypie.authorization import Authorization
from tastypie.resources import ModelResource, Resource
from tastypie.paginator import Paginator
from tastypie.utils import trailing_slash
from tastypie.http import HttpBadRequest
from django.conf import settings
import httplib

import numpy as np

import datetime
import logging

from models import MedicationPlan, WeightMeasurement, HeartRateMeasurement, StepsMeasurement

logger = logging.getLogger("medical_compliance")

def get_measurements_from_store(endpoint_path ="/api/v1/measurement/last_measurements/", **kwargs):
    import requests
    # if not "limit" in kwargs:
    #     kwargs["limit"] = 20
    #
    # if not "order_by" in kwargs:
    #     kwargs["order_by"] = "-timestamp"

    endpoint = settings.STORE_ENDPOINT_URI + endpoint_path
    r = requests.get(endpoint, params=dict(kwargs))

    response_json = r.json()
    return r.status_code, response_json


class MedicationPlanResource(ModelResource):
    class Meta:
        queryset = MedicationPlan.objects.all()
        resource_name = 'medication-plans'
        authentication = Authentication()
        authorization = Authorization()

class BloodPressureMeasurementResource(Resource):
    class Meta:
        resource_name = 'bloodpressure-measurements'
        authentication = Authentication()
        authorization = Authorization()

    def prepend_urls(self):
        return [
            url(r"^(?P<resource_name>%s)/last_values/(?P<bp_element>systolic|diastolic|pulse)%s$" % (self._meta.resource_name, trailing_slash()), self.wrap_view('get_last_values'), name="api_last_values"),
        ]

    def get_last_values(self, request, **kwargs):
        bp_element = kwargs.get("bp_element", None)

        self.method_check(request, allowed=['get'])
        self.is_authenticated(request)
        self.throttle_check(request)

        status, bp_results = self.get_last_values_from_store(request)
        if status != httplib.OK:
            return self.create_response(request, bp_results, response_class=HttpBadRequest)

        amount = []
        data_list = []

        for index, measurement in enumerate(bp_results):
            # amount = [measurement.value] + amount
            amount = [measurement['value_info']['value']] + amount

            data_entry = {}
            # data_entry['timestamp'] = measurement.timestamp
            # data_entry['value'] = measurement.value
            data_entry['timestamp'] = measurement['timestamp']
            data_entry['value'] = measurement['value_info'][bp_element]

            data_entry['status'] = self.compute_bp_status(measurement['value_info'], bp_element)
            data_list = [data_entry] + data_list

        status = "ok"
        threshold = 80

        ## TODO: actual status check for measurement series
        # if len(amount) > 0:
        #     threshold = np.mean(amount)
        #     if threshold < 60 or threshold > 100:
        #         status = "warning"

        jsonResult = {
            "heart_rate": {
                "status": status,
                "amount": amount,
                "data": data_list,
                "threshold": threshold
            }
        }
        return self.create_response(request, jsonResult)


    def compute_bp_status(self, measurement, bp_element):
        ## TODO: actual status check
        return "ok"


    def get_last_values_from_store(self, request):
        filter_dict = dict(type="blood_pressure")

        user = request.GET.get("user", None)
        device = request.GET.get("device", None)

        if user:
            filter_dict['user'] = user

        if device:
            filter_dict['device'] = device

        status, measurements_result = get_measurements_from_store(**filter_dict)
        return status, measurements_result


class HeartRateMeasurementResource(ModelResource):
    class Meta:
        queryset = HeartRateMeasurement.objects.all().order_by('-timestamp')
        resource_name = 'heartrate-measurements'
        paginator_class = Paginator
        authentication = Authentication()
        authorization = Authorization()

    def prepend_urls(self):
        return [
            url(r"^(?P<resource_name>%s)/last_values%s$" % (self._meta.resource_name, trailing_slash()),
                self.wrap_view('get_last_values'), name="api_last_values"),
        ]

    def get_last_values(self, request, **kwargs):
        self.method_check(request, allowed=['get'])
        self.is_authenticated(request)
        self.throttle_check(request)

        # last_hr_measurements = HeartRateMeasurement.objects.all().order_by('-timestamp')[:20]
        status, heartrate_results = self.get_last_values_from_store(request)
        if status != httplib.OK:
            return self.create_response(request, heartrate_results, response_class=HttpBadRequest)

        amount = []
        data_list = []

        for index, measurement in enumerate(heartrate_results):
            # amount = [measurement.value] + amount
            amount = [measurement['value_info']['value']] + amount

            data_entry = {}
            # data_entry['timestamp'] = measurement.timestamp
            # data_entry['value'] = measurement.value
            data_entry['timestamp'] = measurement['timestamp']
            data_entry['value'] = measurement['value_info']['value']

            data_entry['status'] = "ok"
            if data_entry['value'] < 60 or data_entry['value'] > 100:
                data_entry['status'] = "warning"
            data_list = [data_entry] + data_list

        status = "ok"
        threshold = 80

        if len(amount) > 0:
            threshold = np.mean(amount)
            if threshold < 60 or threshold > 100:
                status = "warning"

        jsonResult = {
            "heart_rate": {
                "status": status,
                "amount": amount,
                "data": data_list,
                "threshold": threshold
            }
        }
        return self.create_response(request, jsonResult)

    def get_last_values_from_store(self, request):
        filter_dict = dict(type="pulse")

        user = request.GET.get("user", None)
        device = request.GET.get("device", None)

        if user:
            filter_dict['user'] = user

        if device:
            filter_dict['device'] = device

        status, measurements_results = get_measurements_from_store(**filter_dict)

        if status == httplib.OK:
            for i, meas in enumerate(measurements_results):
                val = None
                if 'Value' in meas['value_info']:
                    val = meas['value_info']['Value']
                else:
                    val = meas['value_info']['value']

                measurements_results[i]['value_info']['value'] = int(val)

        return status, measurements_results

class WeightMeasurementResource(ModelResource):
    class Meta:
        queryset = WeightMeasurement.objects.all().order_by('-timestamp')
        resource_name = 'weight-measurements'
        paginator_class = Paginator
        authentication = Authentication()
        authorization = Authorization()

    def prepend_urls(self):
        return [
            url(r"^(?P<resource_name>%s)/last_values%s$" % (self._meta.resource_name, trailing_slash()), self.wrap_view('get_last_values'), name="api_last_values"),
        ]

    def get_last_values(self, request, **kwargs):
        self.method_check(request, allowed=['get'])
        self.is_authenticated(request)
        self.throttle_check(request)

        #last_weight_measurements = WeightMeasurement.objects.all().order_by('-timestamp')[:20]
        status, weight_results = self.get_last_values_from_store(request)
        if status != httplib.OK:
            return self.create_response(request, weight_results, response_class=HttpBadRequest)

        amount = []
        data_list = []
        
        for index, measurement in enumerate(weight_results):
            #amount = [measurement.value] + amount
            amount = [measurement['value_info']['value']] + amount

            data_entry = {}
            #data_entry['timestamp'] = measurement.timestamp
            data_entry['timestamp'] = measurement['timestamp']

            #data_entry['value'] = measurement.value
            data_entry['value'] = measurement['value_info']['value']

            data_entry['status'] = "ok"
            if index > 0:
                #prev_measurement = last_weight_measurements[index - 1]
                #if abs(measurement.value - prev_measurement.value) >= 2:
                #    data_entry['status'] = "warning"
                prev_measurement = weight_results[index - 1]
                if abs(measurement['value_info']['value'] - prev_measurement['value_info']['value']) >= 2:
                   data_entry['status'] = "warning"
            data_list = [data_entry] + data_list
            
        status = "ok"
        threshold = 80
        
        if len(amount) > 0:
            threshold = np.mean(amount)
            darr = np.array(amount)
            if abs(max(darr) - min(darr)) >= 2:
                status = "warning"

        jsonResult = {
            "weight": {
                "status": status,
                "amount": amount,
                "data": data_list,
                "threshold": threshold
            }
        }
        return self.create_response(request, jsonResult)

    def get_last_values_from_store(self, request):
        filter_dict = dict(type = "weight")

        user = request.GET.get("user", None)
        device = request.GET.get("device", None)

        if user:
            filter_dict['user'] = user

        if device:
            filter_dict['device'] = device

        status, measurements_result = get_measurements_from_store(**filter_dict)

        if status == httplib.OK:
            for i, meas in enumerate(measurements_result):
                val = None
                if 'Value' in meas['value_info']:
                    val = meas['value_info']['Value']
                else:
                    val = meas['value_info']['value']

                measurements_result[i]['value_info']['value'] = float(val)

        return status, measurements_result



class MeasurementTimeResolution(object):
    HOURS         = "hours"
    DAYS          = "days"


class MeasurementTimeFrame(object):
    def __init__(self, start_ts, end_ts):
        self.start_ts = start_ts
        self.end_ts = end_ts
    
    def __str__(self):
        return "{ \"start_ts\": %s, \"end_ts\": %s }" % \
            (self.start_ts, self.end_ts)

class StepsMeasurementResource(ModelResource):
    class Meta:
        queryset = StepsMeasurement.objects.all().order_by('-start_timestamp')
        resource_name = 'steps-measurements'
        paginator_class = Paginator
        authentication = Authentication()
        authorization = Authorization()

    def prepend_urls(self):
        return [
            url(r"^(?P<resource_name>%s)/last_values%s$" % (self._meta.resource_name, trailing_slash()), self.wrap_view('get_last_values'), name="api_last_values"),
        ]

    def get_last_values(self, request, **kwargs):
        self.method_check(request, allowed=['get'])
        self.is_authenticated(request)
        self.throttle_check(request)
        
        try:
            self.check_get_params(request)
        except Exception as e:
            jsonResult = {
                "error": {
                    "message": e
                }
            }
            return self.create_response(request, jsonResult)
        
        resolution =  request.GET.get('resolution', None)
        units = request.GET.get('units', None)

        frames = self.get_last_measurement_time_frames(resolution, int(units))
        
        status = "ok"
        total_amount = 0
        data_list = []

        if len(frames) > 0:
            start_from = frames[-1].start_ts
            start_to = frames[0].end_ts

            # last_steps_measurements = StepsMeasurement.objects.filter(start_timestamp__gte=start_from, start_timestamp__lte=start_to).order_by('-start_timestamp')
            status, steps_results = self.get_steps_from_store(request, start_from, start_to)
            if status != httplib.OK:
                return self.create_response(request, steps_results, response_class=HttpBadRequest)

            logger.debug("[medical-compliance] Filtered steps measurements (%s, %s): %s" % (start_from, start_to, steps_results))

            total_amount = 0
            for frame in frames:
                logger.debug("[medical-compliance] Aggregating frame: %s" % (frame))

                data_entry = {}
                data_entry['start_timestamp'] = frame.start_ts
                data_entry['end_timestamp'] = frame.end_ts
                
                frame_amount = 0
                for measurement in steps_results:
                    logger.debug("[medical-compliance] Check if measurement %s should be aggregated in frame %s" % (measurement, frame))
                    
                    # if measurement.start_timestamp < frame.start_ts or measurement.start_timestamp > frame.end_ts:
                    #     continue

                    if measurement['value_info']['start_timestamp'] < frame.start_ts or measurement['value_info']['start_timestamp'] > frame.end_ts:
                        continue
                    #frame_amount = measurement.value + frame_amount
                    frame_amount = measurement['value_info']['value'] + frame_amount

                data_entry['status'] = "ok" 
                data_entry['value'] = frame_amount
                total_amount = total_amount + frame_amount
                data_list = [data_entry] + data_list

        jsonResult = {
            "steps": {
                "status": status,
                "amount": [total_amount],
                "data": data_list,
            }
        }
        return self.create_response(request, jsonResult)
    
    def check_get_params(self, request):
        resolution =  request.GET.get('resolution', None)
        units = request.GET.get('units', None)

        if units == None or resolution == None:
            raise Exception("units and resolution are manadatory")
        elif resolution != MeasurementTimeResolution.DAYS and resolution != MeasurementTimeResolution.HOURS:
            raise Exception("resolution must be in: %s" % ([MeasurementTimeResolution.DAYS, MeasurementTimeResolution.HOURS]))
    
    def get_last_measurement_time_frames(self, resolution, no_of_units):
        frames = []
        now = datetime.datetime.now()

        if resolution == MeasurementTimeResolution.HOURS:
            current_hour = now.replace(minute=0, second=0)
            
            for hour in range(0, no_of_units):
                start_date = current_hour - datetime.timedelta(hours=hour)
                end_date = start_date + datetime.timedelta(minutes=59, seconds=59)
                
                frames.append(MeasurementTimeFrame(self.get_ts_from_date(start_date), self.get_ts_from_date(end_date)))

        elif resolution == MeasurementTimeResolution.DAYS:
            current_day = now.replace(hour = 0, minute=0, second=0)    

            for day in range(0, no_of_units):
                start_date = current_day - datetime.timedelta(days=day)
                end_date = start_date + datetime.timedelta(hours=23, minutes=59, seconds=59)

                frames.append(MeasurementTimeFrame(self.get_ts_from_date(start_date), self.get_ts_from_date(end_date)))

        return frames

    def get_ts_from_date(self, date):
        return int(
            (
                date - 
                datetime.datetime(1970, 1, 1)
            ).total_seconds()
        )


    def get_steps_from_store(self, request, start_ts, end_ts):
        filter_dict = dict(endpoint_path = "/api/v1/measurement/",
                           measurement_type = "steps",
                           value_info__start_timestamp__gte = start_ts,
                           value_info__start_timestamp__lte = end_ts,
                           order_by = "-timestamp")

        user = request.GET.get("user", None)
        device = request.GET.get("device", None)

        if user:
            filter_dict['user'] = user

        if device:
            filter_dict['device'] = device

        status, steps_data = get_measurements_from_store(**filter_dict)

        #logger.debug("[medical-compliance] Retrieved last steps data: %s " % str(steps_data))

        if status == httplib.OK:
            if 'measurements' in steps_data:
                return status, steps_data['measurements']
            else:
                return status, []

        return status, steps_data
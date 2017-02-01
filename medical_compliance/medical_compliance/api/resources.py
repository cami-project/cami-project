from django.conf.urls import url
from tastypie.authentication import Authentication
from tastypie.authorization import Authorization
from tastypie.resources import ModelResource
from tastypie.paginator import Paginator
from tastypie.utils import trailing_slash
import numpy as np

import datetime
import logging

from models import MedicationPlan, WeightMeasurement, HeartRateMeasurement, StepsMeasurement

logger = logging.getLogger("medical_compliance")

class MedicationPlanResource(ModelResource):
    class Meta:
        queryset = MedicationPlan.objects.all()
        resource_name = 'medication-plans'
        authentication = Authentication()
        authorization = Authorization()


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

        last_weight_measurements = WeightMeasurement.objects.all().order_by('-timestamp')[:20]
        amount = []
        data_list = []
        
        for index, measurement in enumerate(last_weight_measurements):
            amount = [measurement.value] + amount
            data_entry = {}
            data_entry['timestamp'] = measurement.timestamp
            data_entry['value'] = measurement.value
            data_entry['status'] = "ok"
            if index > 0:
                prev_measurement = last_weight_measurements[index - 1]
                if abs(measurement.value - prev_measurement.value) >= 2:
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


class HeartRateMeasurementResource(ModelResource):
    class Meta:
        queryset = HeartRateMeasurement.objects.all().order_by('-timestamp')
        resource_name = 'heartrate-measurements'
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

        last_hr_measurements = HeartRateMeasurement.objects.all().order_by('-timestamp')[:20]
        amount = []
        data_list = []
        
        for index, measurement in enumerate(last_hr_measurements):
            amount = [measurement.value] + amount
            data_entry = {}
            data_entry['timestamp'] = measurement.timestamp
            data_entry['value'] = measurement.value
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
            logger.debug("[medical-compliance] All steps measurements: %s" % (StepsMeasurement.objects.all()))

            start_from = frames[0].start_ts
            start_to = frames[-1].end_ts
            last_steps_measurements = StepsMeasurement.objects.filter(start_timestamp__gte=start_from, start_timestamp__lte=start_to).order_by('-start_timestamp')
            logger.debug("[medical-compliance] Filtered steps measurements (%s, %s): %s" % (start_to, start_from, last_steps_measurements))

            total_amount = 0
            for frame in frames:
                logger.debug("[medical-compliance] Aggregating frame: %s" % (frame))

                data_entry = {}
                data_entry['start_timestamp'] = frame.start_ts
                data_entry['end_timestamp'] = frame.end_ts
                
                frame_amount = 0
                for measurement in last_steps_measurements:
                    logger.debug("[medical-compliance] Check if measurement %s should be aggregated in frame %s" % (measurement, frame))
                    
                    if measurement.start_timestamp < frame.start_ts or measurement.start_timestamp > frame.end_ts:
                        continue
                    frame_amount = [measurement.value] + frame_amount    

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
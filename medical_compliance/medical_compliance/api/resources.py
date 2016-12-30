from django.conf.urls import url
from tastypie.authentication import Authentication
from tastypie.authorization import Authorization
from tastypie.resources import ModelResource
from tastypie.paginator import Paginator
from tastypie.utils import trailing_slash
import numpy as np

from models import MedicationPlan, WeightMeasurement, HeartRateMeasurement


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
from django.conf.urls import url
from tastypie.authentication import Authentication
from tastypie.authorization import Authorization
from tastypie.resources import ModelResource
from tastypie.paginator import Paginator
from tastypie.utils import trailing_slash
import numpy as np

from models import MedicationPlan, WeightMeasurement


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

        lastWeightMeasurements = WeightMeasurement.objects.all().order_by('-timestamp')[:5]
        amount = []
        for measurement in lastWeightMeasurements:
            amount = [measurement.value] + amount

        status = "ok"
        if len(amount) > 0:
            darr = np.array(amount)
            if abs(max(darr) - min(darr)) >= 2:
                status = "warning"

        jsonResult = {
            "weight": {
                "status": status,
                "amount": amount
            }
        }
        return self.create_response(request, jsonResult)

from tastypie.authentication import Authentication
from tastypie.authorization import Authorization
from tastypie.resources import ModelResource

from models import MedicationPlan, MeasurementNotification


class MedicationPlanResource(ModelResource):
    class Meta:
        queryset = MedicationPlan.objects.all()
        resource_name = 'medication-plans'
        authentication = Authentication()
        authorization = Authorization()


class MeasurementNotificationResource(ModelResource):
    class Meta:
        queryset = MeasurementNotification.objects.all()
        resource_name = 'measurement-notifications'
        authentication = Authentication()
        authorization = Authorization()

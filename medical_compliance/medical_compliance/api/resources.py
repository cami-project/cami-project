from tastypie.authentication import Authentication
from tastypie.authorization import Authorization
from tastypie.resources import ModelResource

from models import MedicationPlan


class MedicationPlanResource(ModelResource):
    class Meta:
        queryset = MedicationPlan.objects.all()
        resource_name = 'medication-plans'
        authentication = Authentication()
        authorization = Authorization()

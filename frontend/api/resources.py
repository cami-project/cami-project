from tastypie.authentication import Authentication
from tastypie.authorization import Authorization
from tastypie.resources import ModelResource

from models import Notification


class NotificationResource(ModelResource):
    class Meta:
        queryset = Notification.objects.all()
        resource_name = 'notifications'
        authentication = Authentication()
        authorization = Authorization()

import logging

from tastypie import fields
from tastypie.authentication import Authentication
from tastypie.authorization import Authorization
from tastypie.resources import ModelResource, Resource
from tastypie.serializers import Serializer
from tastypie.paginator import Paginator

from models import Notification, MobileNotificationKey

from django.utils.timezone import is_naive

from healthchecker import Healthchecker

# Get an instance of a logger
logger = logging.getLogger(__name__)


class TZAwareDateSerializer(Serializer):
    """
    Our own serializer to format datetimes in ISO 8601 but with timezone
    offset.
    """

    def format_datetime(self, data):
        logger.info(data)
        # If naive or rfc-2822, default behavior...
        if is_naive(data) or self.datetime_formatting == 'rfc-2822':
            return super(TZAwareDateSerializer, self).format_datetime(data)

        return data.isoformat()


class NoMetaPaginator(Paginator):
    """
    Our own paginator to hide the metadata
    """

    def page(self):
        res = super(NoMetaPaginator, self).page()
        del res['meta']
        return res


class ApiModelResource(ModelResource):
    class Meta:
        authentication = Authentication()
        authorization = Authorization()
        serializer = TZAwareDateSerializer()


class NotificationResource(ApiModelResource):
    class Meta(ApiModelResource.Meta):
        queryset = Notification.objects.all().order_by('-timestamp')
        resource_name = 'notifications'
        filtering = {
            "timestamp": ('gt'),
            "recipient_type": ('exact')
        }
        paginator_class = Paginator


class HealthcheckResource(Resource):
    status = fields.CharField(attribute='status', readonly=True)
    mysql = fields.CharField(attribute='mysql', readonly=True)
    message_queue = fields.CharField(attribute='message_queue', readonly=True)

    class Meta:
        resource_name = 'healthcheck'
        collection_name = 'healthcheck'
        allowed_methods = ['get']
        include_resource_uri = False
        paginator_class = NoMetaPaginator
        object_class = Healthchecker

    def detail_uri_kwargs(self, bundle_or_obj):
        kwargs = {}
        return kwargs

    def get_object_list(self, request):
        return [self.obj_get()]

    def obj_get_list(self, request=None, **kwargs):
        return [self.obj_get()]

    def obj_get(self, request=None, key=None, **kwargs):
        healthcheck = Healthchecker()
        return healthcheck


class MobileNotificationKeyResource(Resource):
    class Meta:
        resource_name = 'mobile-notification-key'
        allowed_methods = ['post']
        authentication = Authentication()
        authorization = Authorization()
        object_class = MobileNotificationKey

    def obj_create(self, bundle, **kwargs):

        if bundle.data.has_key("mobile_key") and bundle.data.has_key("mobile_os"):
            mobile_key = bundle.data.get("mobile_key")
            mobile_os = bundle.data.get("mobile_os")
            
            # TODO: to be changed to a user id form the session
            dummy_user_id = 11262861
            dummy_recipient_type = "caregiver"

            notification_key, created = MobileNotificationKey.objects.get_or_create(
                user_id=dummy_user_id,
                recipient_type=dummy_recipient_type,
                mobile_key=mobile_key,
                mobile_os=mobile_os
            )
            return notification_key
        return bundle
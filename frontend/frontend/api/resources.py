import logging

from datetime import datetime

from django.conf.urls import url
from django.utils.timezone import is_naive

from tastypie import fields
from tastypie.utils import trailing_slash
from tastypie.paginator import Paginator
from tastypie.resources import ModelResource, Resource
from tastypie.serializers import Serializer
from tastypie.authorization import Authorization
from tastypie.authentication import Authentication

# Local imports
import frontend.store_utils as store_utils

from healthchecker import Healthchecker


# Get an instance of a logger
logger = logging.getLogger(__name__)


class NoMetaPaginator(Paginator):
    """
    Our own paginator to hide the metadata
    """

    def page(self):
        res = super(NoMetaPaginator, self).page()
        del res['meta']
        return res


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


class PushNotificationSubscribeResource(Resource):
    class Meta:
        resource_name = 'push-notification-subscribe'
        allowed_methods = ['post']
        authentication = Authentication()
        authorization = Authorization()
        always_return_data = True

    def detail_uri_kwargs(self, bundle_or_obj):
        kwargs = {}
        kwargs['pk'] = bundle_or_obj.data['registration_id']
        return kwargs

    def obj_create(self, bundle, **kwargs):
        if (
            bundle.data.has_key("registration_id") and
            bundle.data.has_key("service_type") and
            bundle.data.has_key("user_id")
        ):
            registration_id = bundle.data.get("registration_id")
            service_type = bundle.data.get("service_type")
            user_id = bundle.data.get("user_id")

            bundle.data = store_utils.pushnotificationdevice_save(
                user="/api/v1/user/%d/" % user_id,
                type=service_type,
                registration_id=registration_id
            )

        return bundle

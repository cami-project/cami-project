import ast
import json
import time
import datetime

from tastypie.resources import ModelResource
from tastypie.authorization import Authorization
from tastypie import fields
from django.contrib.auth.models import User
from django.conf.urls import url
from tastypie.exceptions import NotFound
from tastypie.utils import trailing_slash
from django.core.urlresolvers import resolve, get_script_prefix, Resolver404
from tastypie.constants import ALL, ALL_WITH_RELATIONS
from django.core import serializers

from store.models import EndUserProfile, Device, DeviceUsage, Measurement, Activity


class UserResource(ModelResource):
    devices = fields.ToManyField('store.api.resources.DeviceResource', 'used_devices')

    class Meta:
        excludes = ['password', 'is_staff', 'is_superuser', 'email']
        queryset = User.objects.all()
        allowed_methods = ['get']
        collection_name = "users"


class DeviceResource(ModelResource):
    class Meta:
        queryset = Device.objects.all()
        allowed_methods = ['get']
        collection_name = "devices"

        filtering = {
            'id' : ('exact', ),
            'model': ('exact',),
            'manufacturer': ('exact',),
            'serial_number': ('exact',)
        }


class DeviceUsageResource(ModelResource):
    ACCESS_INFO_FIELD_NAME = 'access_info'

    user = fields.ToOneField(UserResource, 'user')
    device = fields.ToOneField(DeviceResource, 'device')

    class Meta:
        queryset = DeviceUsage.objects.all()
        allowed_methods = ['get']
        filtering = {
            "user": ALL_WITH_RELATIONS,
            "device" : ALL_WITH_RELATIONS,
            "access_info": ALL
        }


    def dehydrate(self, bundle):
        '''
        By default the access_info JSONField is retrieved from the DB as a unicode string.
        Use ast.literal_eval to turn string representation of dict into actual Python dict
        '''
        if DeviceUsageResource.ACCESS_INFO_FIELD_NAME in bundle.data:
            access_info_str = bundle.data[DeviceUsageResource.ACCESS_INFO_FIELD_NAME]
            access_info_dict = ast.literal_eval(access_info_str)     # fix single quoted unicode keys in access_info serialization

            bundle.data[DeviceUsageResource.ACCESS_INFO_FIELD_NAME] = access_info_dict

        return bundle


    def build_filters(self, filters = None):
        if filters is None:
            filters = {}

        access_info_filters = {k : v for k,v in filters.items() if k.startswith(DeviceUsageResource.ACCESS_INFO_FIELD_NAME)}
        remaining_filter = {k : v for k, v in filters.items() if k not in access_info_filters}

        orm_filters = super(DeviceUsageResource, self).build_filters(remaining_filter)
        orm_filters.update(access_info_filters)

        return orm_filters


class MeasurementResource(ModelResource):
    VALUE_INFO_FIELD_NAME   = "value_info"
    CONTEXT_INFO_FIELD_NAME = "context_info"

    user = fields.ForeignKey(UserResource, 'user')
    device = fields.ForeignKey(DeviceResource, 'device')

    class Meta:
        queryset = Measurement.objects.all()
        allowed_methods = ['get', 'post', 'put']
        collection_name = "measurements"

        authorization = Authorization()
        always_return_data = True

        ordering = ["timestamp"]
        filtering = {
            "id": ALL,
            "measurement_type": ('exact', 'iexact', 'in'),
            "user": ALL_WITH_RELATIONS,
            "device": ALL_WITH_RELATIONS,
            "timestamp": ALL,
            "value_info": ALL,
            "context_info": ALL
        }

    def dehydrate_timestamp(self, bundle):
        return int(bundle.data['timestamp'])


    def dehydrate(self, bundle):
        '''
        By default the value_info JSONField is retrieved from the DB as a unicode string.
        Use ast.literal_eval to turn string representation of dict into actual Python dict
        '''
        if MeasurementResource.VALUE_INFO_FIELD_NAME in bundle.data:
            value_info_str = bundle.data[MeasurementResource.VALUE_INFO_FIELD_NAME]
            value_info_dict = ast.literal_eval(value_info_str)

            bundle.data[MeasurementResource.VALUE_INFO_FIELD_NAME] = value_info_dict

        if MeasurementResource.CONTEXT_INFO_FIELD_NAME in bundle.data:
            context_info_str = bundle.data[MeasurementResource.CONTEXT_INFO_FIELD_NAME]
            context_info_dict = ast.literal_eval(context_info_str)

            bundle.data[MeasurementResource.CONTEXT_INFO_FIELD_NAME] = context_info_dict

        return bundle


    def build_filters(self, filters = None):
        '''
        The double underscores are for the JSONFields value_info and context_info are interpreted
        by Tastypie as relational filters.
        To include them as JSONField filters, we first remove them from the filter list,
        run the superclass (Tastypie default) filter building and then add them back, such that
        they will be interpreted as Django ORM filters.
        '''
        if filters is None:
            filters = {}

        info_filters = {k : v for k, v in filters.items()
                        if k.startswith(MeasurementResource.VALUE_INFO_FIELD_NAME) or
                        k.startswith(MeasurementResource.CONTEXT_INFO_FIELD_NAME)}
        remaining_filter = {k : v for k, v in filters.items() if k not in info_filters}

        orm_filters = super(MeasurementResource, self).build_filters(remaining_filter)
        orm_filters.update(info_filters)

        return orm_filters


class ActivityResource(ModelResource):
    user = fields.ForeignKey(UserResource, 'user')

    class Meta:
        queryset = Activity.objects.all()
        allowed_methods = ['get']
        collection_name = "activities"

        authorization = Authorization()
        always_return_data = True

        ordering = ["start"]
        filtering = {
            "user": ALL_WITH_RELATIONS,
            "start": ALL,
            "end": ALL,
            "activity_type": ALL
        }

    def prepend_urls(self):
        return [
            url(r"^(?P<resource_name>%s)/last_activities%s$" % (self._meta.resource_name, trailing_slash()), self.wrap_view('get_last_activities'), name="api_last_activities"),
        ]

    def get_last_activities(self, request, **kwargs):
        self.method_check(request, allowed=['get'])
        self.is_authenticated(request)
        self.throttle_check(request)

        date_start = datetime.datetime.now() - datetime.timedelta(days=7)
        date_end = datetime.datetime.now() + datetime.timedelta(days=7)

        last_activities = Activity.objects.all().filter(
            start__gte=time.mktime(date_start.timetuple()),
            end__lte=time.mktime(date_end.timetuple())
        ).order_by('start')

        return self.create_response(request, list(last_activities.values()))


def get_pk_from_uri(uri):
    prefix = get_script_prefix()
    chomped_uri = uri

    if prefix and chomped_uri.startswith(prefix):
       chomped_uri = chomped_uri[len(prefix) - 1:]

    try:
        view, args, kwargs = resolve(chomped_uri)
    except Resolver404:
        raise NotFound("The URL provided '%s' was not a link to a valid resource." % uri)

    return kwargs['pk']
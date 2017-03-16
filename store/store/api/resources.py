from tastypie.resources import ModelResource
from tastypie.authorization import Authorization
from tastypie import fields
from django.contrib.auth.models import User
from tastypie.exceptions import NotFound
from django.core.urlresolvers import resolve, get_script_prefix, Resolver404
from tastypie.constants import ALL, ALL_WITH_RELATIONS
import json, ast

from store.models import EndUserProfile, Device, DeviceUsage, Measurement


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
            'id' : ('exact', )
        }


class DeviceUsageResource(ModelResource):
    ACCESS_INFO_FILTER_NAME = 'access_q'
    ACCESS_INFO_FIELD_NAME = 'access_info'

    user = fields.ToOneField(UserResource, 'user')
    device = fields.ToOneField(DeviceResource, 'device')

    class Meta:
        queryset = DeviceUsage.objects.all()
        allowed_methods = ['get']
        filtering = {
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
            "measurement_type": ('exact', 'iexact', 'in'),
            "user": ALL_WITH_RELATIONS,
            "device": ALL_WITH_RELATIONS,
            "timestamp": ALL,
            "value_info": ALL,
            "context_info": ALL
        }


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
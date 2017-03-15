from tastypie.resources import ModelResource
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


class DeviceResource(ModelResource):
    class Meta:
        queryset = Device.objects.all()
        allowed_methods = ['get']
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
    user = fields.ToOneField(UserResource, 'user')
    device = fields.ToOneField(DeviceResource, 'device')

    class Meta:
        queryset = Measurement.objects.all()
        allowed_methods = ['get', 'post', 'put']


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
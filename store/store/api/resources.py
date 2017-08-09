import ast
import json
import time
import datetime

from tastypie import fields
from tastypie.utils import trailing_slash
from tastypie.resources import ModelResource
from tastypie.constants import ALL, ALL_WITH_RELATIONS
from tastypie.paginator import Paginator
from tastypie.exceptions import NotFound
from tastypie.authorization import Authorization
from tastypie.authentication import Authentication

from django.core import serializers
from django.conf.urls import url
from django.core.urlresolvers import resolve, get_script_prefix, Resolver404
from django.contrib.auth.models import User

from store.models import EndUserProfile, Device, DeviceUsage, \
    Measurement, Activity, JournalEntry, PushNotificationDevice


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

    def build_filters(self, filters=None, ignore_bad_filters=True):
        if filters is None:
            filters = {}

        access_info_filters = {k : v for k,v in filters.items() if k.startswith(DeviceUsageResource.ACCESS_INFO_FIELD_NAME)}
        remaining_filter = {k : v for k, v in filters.items() if k not in access_info_filters}

        orm_filters = super(DeviceUsageResource, self).build_filters(remaining_filter)
        orm_filters.update(access_info_filters)

        return orm_filters


class MeasurementResource(ModelResource):
    VALUE_INFO_FIELD_NAME = "value_info"

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

        return bundle

    def build_filters(self, filters=None, ignore_bad_filters=True):
        '''
        The double underscores are for the JSONFields value_info are interpreted
        by Tastypie as relational filters.
        To include them as JSONField filters, we first remove them from the filter list,
        run the superclass (Tastypie default) filter building and then add them back, such that
        they will be interpreted as Django ORM filters.
        '''
        if filters is None:
            filters = {}

        info_filters = {
            k : v
            for k, v in filters.items()
            if k.startswith(MeasurementResource.VALUE_INFO_FIELD_NAME)
        }
        remaining_filter = {k : v for k, v in filters.items() if k not in info_filters}

        orm_filters = super(MeasurementResource, self).build_filters(remaining_filter)
        orm_filters.update(info_filters)

        return orm_filters

    def prepend_urls(self):
        return [
            url(
                r"^(?P<resource_name>%s)/last_measurements%s$" % (
                    self._meta.resource_name,
                    trailing_slash()
                ),
                self.wrap_view('get_last_measurements'),
                name="api_last_measurements"
            ),
        ]

    def get_last_measurements(self, request, **kwargs):
        self.method_check(request, allowed=['get'])
        self.is_authenticated(request)
        self.throttle_check(request)

        try:
            self.check_get_params(request)
        except Exception as e:
            json_error = {
                "error": {
                    "message": e
                }
            }
            return self.create_response(request, json_error)

        type = request.GET.get('type', None)

        last_measurements = Measurement.objects.all().filter(
            measurement_type = type
        ).order_by('-timestamp')[:20]

        return self.create_response(request, list(last_measurements.values()))

    def check_get_params(self, request):
        type = request.GET.get('type', None)
        measurements = dict(Measurement.MEASUREMENTS).keys()

        if type == None:
            raise Exception(
                "measurement type is manadatory, and must be one of: %s" % (
                    ", ".join(measurements)
                )
            )
        elif type not in measurements:
            raise Exception("measurement type must be one of: %s" % ", ".join(measurements))


class ActivityResource(ModelResource):
    user = fields.ForeignKey(UserResource, 'user')

    class Meta:
        queryset = Activity.objects.all()
        allowed_methods = ['get', 'post', 'put', 'delete']
        collection_name = "activities"

        authorization = Authorization()
        always_return_data = True

        ordering = ["start"]
        filtering = {
            "id": ALL,
            "user": ALL_WITH_RELATIONS,
            "start": ALL,
            "end": ALL,
            "activity_type": ALL,
            "calendar_id": ALL,
            "reminders": ALL,
        }

    def prepend_urls(self):
        return [
            url(
                r"^(?P<resource_name>%s)/last_activities%s$" % (
                    self._meta.resource_name,
                    trailing_slash()
                ),
                self.wrap_view('get_last_activities'),
                name="api_last_activities"
            ),
        ]

    def dehydrate(self, bundle):
        bundle.data['start'] = int(bundle.data['start'])
        bundle.data['end'] = int(bundle.data['end'])
        bundle.data['created'] = int(bundle.data['created'])
        bundle.data['updated'] = int(bundle.data['updated'])

        bundle.data['color'] = ast.literal_eval(bundle.data['color'])
        bundle.data['reminders'] = ast.literal_eval(bundle.data['reminders'])
        bundle.data['creator'] = ast.literal_eval(bundle.data['creator'])

        return bundle

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


class JournalEntryResource(ModelResource):
    user = fields.ForeignKey(UserResource, 'user')
    
    class Meta:
        authentication = Authentication()
        authorization = Authorization()
        always_return_data = True
        queryset = JournalEntry.objects.all().order_by('-timestamp')
        resource_name = 'journal_entries'
        paginator_class = Paginator

        filtering = {
            "user": ALL_WITH_RELATIONS,
            "timestamp": ('gt'),
            "recipient_type": ('exact')
        }


class PushNotificationDeviceResource(ModelResource):
    user = fields.ForeignKey(UserResource, 'user')

    class Meta:
        authentication = Authentication()
        authorization = Authorization()
        always_return_data = True
        queryset = PushNotificationDevice.objects.all()
        allowed_methods = ['get', 'post', 'put', 'delete']

        filtering = {
            'id' : ('exact'),
            'name': ALL,
            'user': ALL_WITH_RELATIONS,
            'device_id': ('exact'),
            'registration_id': ('exact'),
        }


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

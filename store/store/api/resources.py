import ast
import time

from tastypie import fields
from tastypie.utils import trailing_slash
from tastypie.resources import ModelResource
from tastypie.constants import ALL, ALL_WITH_RELATIONS
from tastypie.paginator import Paginator
from tastypie.exceptions import NotFound
from tastypie.authorization import Authorization
from tastypie.authentication import Authentication
from tastypie.http import HttpBadRequest

from django.conf.urls import url
from django.core.urlresolvers import resolve, get_script_prefix, Resolver404
from django.contrib.auth.models import User

from store.models import *

import logging

logger = logging.getLogger("store")

class UserResource(ModelResource):
    devices = fields.ToManyField('store.api.resources.DeviceResource', 'used_devices')
    enduser_profile = fields.ToOneField('store.api.resources.EndUserProfileResource',
                                        'enduser_profile',
                                         null=True, blank=True, full=True)
    caregiver_profile = fields.ToOneField('store.api.resources.CaregiverProfileResource',
                                        'caregiver_profile',
                                         null=True, blank=True, full=True)

    class Meta:
        excludes = ['password', 'is_staff', 'is_superuser']
        queryset = User.objects.all()
        allowed_methods = ['get']
        collection_name = "users"

        filtering = {
            'username': ('exact',),
            'email': ('exact',),
        }

    def dehydrate(self, bundle):
        # clean out fields that have a null value from returned serialization
        for key in bundle.data.keys():
            if not bundle.data[key]:
                del bundle.data[key]

        return bundle



class EndUserProfileResource(ModelResource):
    user = fields.ToOneField(UserResource, 'user')
    caregivers = fields.ToManyField(UserResource,
            attribute = lambda bundle: User.objects.filter(id__in=map(lambda q: q.user.id, bundle.obj.user.caregivers.all())),
            related_name='caretaker',
            null=True, blank=True)

    class Meta:
        # we exclude the following fields because we don'really have any data, nor do we use them
        excludes = ['marital_status', 'age', 'height', 'phone', 'address']

        queryset = EndUserProfile.objects.all()
        allowed_methods = ['get']
        collection_name = "enduser_profiles"
        filtering = {
            'user': ('exact',),
        }



class CaregiverProfileResource(ModelResource):
    user = fields.ToOneField(UserResource, 'user')
    caretaker = fields.ToOneField(UserResource, 'caretaker')

    class Meta:
        # we exclude the following fields because we don'really have any data, nor do we use them
        excludes = ['phone', 'address']

        queryset = CaregiverProfile.objects.all()
        allowed_methods = ['get']
        collection_name = "caregiver_profiles"
        filtering = {
            'user': ('exact',),
        }

class DeviceResource(ModelResource):
    gateway = fields.ToOneField("store.api.resources.GatewayResource", "gateway", null=True, blank=True)

    class Meta:
        queryset = Device.objects.all()
        allowed_methods = ['get', 'post', 'put']
        collection_name = "devices"
        always_return_data = True
        authorization = Authorization()

        filtering = {
            'id' : ('exact', ),
            'model': ('exact',),
            'manufacturer': ('exact',),
            'serial_number': ('exact',),
            'device_identifier': ('exact',),
            'device_type': ('exact',),
            'gateway': ALL_WITH_RELATIONS,
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


class ExternalServiceResource(ModelResource):
    ACCESS_INFO_FIELD_NAME = 'access_info'

    user = fields.ToOneField(UserResource, 'user')
    class Meta:
        queryset = ExternalService.objects.all()
        #allowed_methods = ['get', 'post']
        ordering = ["updated_at"]
        filtering = {
            "user": ALL_WITH_RELATIONS,
            "name" : ALL,
            "updated_at": ALL,
            "access_info": ALL
        }


class GatewayResource(ModelResource):
    user = fields.ForeignKey(UserResource, 'user')
    devices = fields.ToManyField(DeviceResource, 'connected_devices')

    class Meta:
        queryset = Gateway.objects.all()
        allowed_methods = ['get', 'post', 'put']
        always_return_data = True
        authorization = Authorization()

        filtering = {
            'user' : ALL_WITH_RELATIONS,
            'device_id': ALL
        }


class MeasurementResource(ModelResource):
    VALUE_INFO_FIELD_NAME = "value_info"

    user = fields.ForeignKey(UserResource, 'user')
    device = fields.ForeignKey(DeviceResource, 'device', null=True)
    gateway = fields.ForeignKey(GatewayResource, 'gateway', null=True)

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
            "gateway": ALL_WITH_RELATIONS
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
            # TODO: this is an elegant hack, but still a hack.
            # It because Django JSONField filtering will not handle comparisons with the string form of numbers,
            # where the JSONField actually stores numeric (int, float) values
            k : ast.literal_eval(v)
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
            return self.create_response(request, json_error, response_class=HttpBadRequest)

        type = request.GET.get('type', None)
        user_id = request.GET.get('user', None)
        device_id = request.GET.get('device', None)

        filter_dict = dict( measurement_type = type, user__id = int(user_id) )
        if device_id:
            filter_dict['device__id'] = int(device_id)

        last_measurements = Measurement.objects.all().filter(
            **filter_dict
        ).order_by('-timestamp')[:20]

        return self.create_response(request, list(last_measurements.values()))


    def check_get_params(self, request):
        type = request.GET.get('type', None)
        user_id = request.GET.get('user', None)

        measurements = dict(Measurement.MEASUREMENTS).keys()

        if type == None:
            raise Exception(
                "measurement type is manadatory, and must be one of: %s" % (
                    ", ".join(measurements)
                )
            )
        elif type not in measurements:
            raise Exception("measurement type must be one of: %s" % ", ".join(measurements))
        elif user_id == None:
            raise Exception("user id is manadatory")


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

        user_id = None
        if "user" in request.GET:
            user_id = int(request.GET["user"])
        elif "user" in kwargs:
            user_id = int(kwargs["user"])


        if user_id:
            last_activities = Activity.objects.filter(
                user=user_id,
                start__gte=time.mktime(date_start.timetuple()),
                end__lte=time.mktime(date_end.timetuple())
            ).order_by('start')

            logger.debug("[store] Retrieving last activities for user id: " + str(user_id) + " : %s" % last_activities)

            return self.create_response(request, list(last_activities.values()))
        else:
            last_activities = Activity.objects.filter(
                start__gte=time.mktime(date_start.timetuple()),
                end__lte=time.mktime(date_end.timetuple())
            ).order_by('start')

            logger.debug("[store] Retrieving last activities for user id: " + str(user_id) + " : %s" % last_activities)
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

        ordering = ["timestamp"]
        filtering = {
            "user": ALL_WITH_RELATIONS,
            "timestamp": ("exact", "gt", "gte", "lt", "lte",),
            "type": ('exact',),
            "acknowledged": ('exact', "in")
        }


    def dehydrate(self, bundle):
        ## remove reference_id from serialization if it doesn't have a value
        if "reference_id" in bundle.data and not bundle.data["reference_id"]:
            del bundle.data["reference_id"]

        return bundle


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

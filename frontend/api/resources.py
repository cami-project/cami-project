import logging

from tastypie.authentication import Authentication
from tastypie.authorization import Authorization
from tastypie.resources import ModelResource
from tastypie.serializers import Serializer
from tastypie.paginator import Paginator

from models import Notification

from django.utils.timezone import is_naive

# Get an instance of a logger
logger = logging.getLogger('django')


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
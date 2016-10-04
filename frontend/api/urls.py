from django.conf.urls import include, url
from tastypie.api import Api

from resources import NotificationResource, HealthcheckResource

v1_api = Api(api_name='v1')
v1_api.register(NotificationResource())
v1_api.register(HealthcheckResource())

urlpatterns = [
    url(r'^', include(v1_api.urls)),
]

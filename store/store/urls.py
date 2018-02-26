"""store URL Configuration

The `urlpatterns` list routes URLs to views. For more information please see:
    https://docs.djangoproject.com/en/1.9/topics/http/urls/
Examples:
Function views
    1. Add an import:  from my_app import views
    2. Add a URL to urlpatterns:  url(r'^$', views.home, name='home')
Class-based views
    1. Add an import:  from other_app.views import Home
    2. Add a URL to urlpatterns:  url(r'^$', Home.as_view(), name='home')
Including another URLconf
    1. Import the include() function: from django.conf.urls import url, include
    2. Add a URL to urlpatterns:  url(r'^blog/', include('blog.urls'))
"""
from django.contrib import admin
from django.conf.urls import url, include

from tastypie.api import Api

from store.api.resources import *


v1_api = Api(api_name='v1')
v1_api.register(UserResource())
v1_api.register(EndUserProfileResource())
v1_api.register(CaregiverProfileResource())

v1_api.register(DeviceResource())
v1_api.register(DeviceUsageResource())
v1_api.register(ExternalService())
v1_api.register(MeasurementResource())
v1_api.register(ActivityResource())
v1_api.register(JournalEntryResource())
v1_api.register(PushNotificationDeviceResource())
v1_api.register(GatewayResource())

urlpatterns = [
    url(r'^admin/', admin.site.urls),
    url(r'^api/', include(v1_api.urls)),
    url(
        r'^api/documentation/',
        include('tastypie_swagger.urls', namespace='store_tastypie_swagger'),
        kwargs={
            "tastypie_api_module": v1_api,
            "namespace": "store_tastypie_swagger",
            "version": "0.1"
        }
    ),
]

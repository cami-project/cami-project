"""medical_compliance URL Configuration

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
from django.conf.urls import include, url
from django.contrib import admin
from tastypie.api import Api

from api.resources import MedicationPlanResource, MeasurementNotificationResource
from api import views

v1_api = Api(api_name='v1')
v1_api.register(MedicationPlanResource())
v1_api.register(MeasurementNotificationResource())

urlpatterns = [
    url(r'^api/', include(v1_api.urls)),
    url(r'^admin/', admin.site.urls),
    url(r'^subscribe_notifications/', views.subscribe_notifications, name="subscribe_notifications"),
    url(r'^unsubscribe_notifications/', views.unsubscribe_notifications, name="unsubscribe_notifications"),
    url(r'^notify_measurements/', views.notify_measurements, name="notify_measurements"),
]

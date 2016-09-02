# Create your views here.
from django.shortcuts import render

from django.http import HttpResponse
from django.conf import settings #noqa
from withings import WithingsApi, WithingsCredentials
from .resources import MeasurementNotificationResource
import json

def subscribe_notifications(request):
    credentials = WithingsCredentials(access_token=settings.WITHINGS_OAUTH_V1_TOKEN,
                                      access_token_secret=settings.WITHINGS_OAUTH_V1_TOKEN_SECRET,
                                      consumer_key=settings.WITHINGS_CONSUMER_KEY,
                                      consumer_secret=settings.WITHINGS_CONSUMER_SECRET,
                                      user_id=settings.WITHINGS_USER_ID)
    client = WithingsApi(credentials)
    FULL_CALLBACK_URL = request.build_absolute_uri(MeasurementNotificationResource().get_resource_uri())

    response_data = client.subscribe(FULL_CALLBACK_URL, "Subscribe for weight measurement notifications.", appli=1)
    return HttpResponse(json.dumps(response_data), content_type="application/json")

def unsubscribe_notifications(request):
    credentials = WithingsCredentials(access_token=settings.WITHINGS_OAUTH_V1_TOKEN,
                                      access_token_secret=settings.WITHINGS_OAUTH_V1_TOKEN_SECRET,
                                      consumer_key=settings.WITHINGS_CONSUMER_KEY,
                                      consumer_secret=settings.WITHINGS_CONSUMER_SECRET,
                                      user_id=settings.WITHINGS_USER_ID)
    client = WithingsApi(credentials)

    FULL_CALLBACK_URL = request.build_absolute_uri(MeasurementNotificationResource().get_resource_uri())

    response_data = client.unsubscribe(FULL_CALLBACK_URL, appli=1)
    return HttpResponse(json.dumps(response_data), content_type="application/json")

def notify_measurements(request):
    return HttpResponse(status=200)

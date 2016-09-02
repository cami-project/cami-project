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
    CALLBACK_URL = MeasurementNotificationResource().get_resource_uri()
    print CALLBACK_URL

    response_data = client.subscribe(CALLBACK_URL, "Subscribe for weight measurement notifications.", appli=1)
    return HttpResponse(json.dumps(response_data), content_type="application/json")

def unsubscribe_notifications(request):
    credentials = WithingsCredentials(access_token=settings.WITHINGS_OAUTH_V1_TOKEN,
                                      access_token_secret=settings.WITHINGS_OAUTH_V1_TOKEN_SECRET,
                                      consumer_key=settings.WITHINGS_CONSUMER_KEY,
                                      consumer_secret=settings.WITHINGS_CONSUMER_SECRET,
                                      user_id=settings.WITHINGS_USER_ID)
    client = WithingsApi(credentials)

    CALLBACK_URL = MeasurementNotificationResource().get_resource_uri()
    print CALLBACK_URL

    response_data = client.unsubscribe(CALLBACK_URL, appli=1)
    return HttpResponse(json.dumps(response_data), content_type="application/json")

def notify_measurements(request):
    return HttpResponse(status=200)

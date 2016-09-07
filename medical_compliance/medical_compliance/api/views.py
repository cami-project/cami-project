# Create your views here.
from django.shortcuts import render

from django.http import HttpResponse
from django.conf import settings #noqa
from django.views.decorators.csrf import csrf_exempt
from withings import WithingsApi, WithingsCredentials
from tasks import fetch_measurement
# from .resources import MeasurementNotificationResource

import json, logging, pprint

logger = logging.getLogger("medical_compliance.measurement_callback")

FULL_CALLBACK_URL = 'http://46.101.163.224:8000/notify_measurements/'


def subscribe_notifications(request):
    credentials = WithingsCredentials(access_token=settings.WITHINGS_OAUTH_V1_TOKEN,
                                      access_token_secret=settings.WITHINGS_OAUTH_V1_TOKEN_SECRET,
                                      consumer_key=settings.WITHINGS_CONSUMER_KEY,
                                      consumer_secret=settings.WITHINGS_CONSUMER_SECRET,
                                      user_id=settings.WITHINGS_USER_ID)
    client = WithingsApi(credentials)
    #FULL_CALLBACK_URL = request.build_absolute_uri(MeasurementNotificationResource().get_resource_uri())

    response_data = client.subscribe(FULL_CALLBACK_URL, "Subscribe for weight measurement notifications.", appli=1)
    return HttpResponse(json.dumps(response_data), content_type="application/json")

def unsubscribe_notifications(request):
    credentials = WithingsCredentials(access_token=settings.WITHINGS_OAUTH_V1_TOKEN,
                                      access_token_secret=settings.WITHINGS_OAUTH_V1_TOKEN_SECRET,
                                      consumer_key=settings.WITHINGS_CONSUMER_KEY,
                                      consumer_secret=settings.WITHINGS_CONSUMER_SECRET,
                                      user_id=settings.WITHINGS_USER_ID)
    client = WithingsApi(credentials)

    #FULL_CALLBACK_URL = request.build_absolute_uri(MeasurementNotificationResource().get_resource_uri())

    response_data = client.unsubscribe(FULL_CALLBACK_URL, appli=1)
    return HttpResponse(json.dumps(response_data), content_type="application/json")

@csrf_exempt
def notify_measurements(request):
    logger.debug(pprint.pformat(request.POST))
    # TODO: make this nicer and more error proof (i.e. check for existance of fields in post data)
    fetch_measurement.delay(
        request.POST['userid'],
        request.POST['startdate'],
        request.POST['enddate'],
        request.POST['appli'])

    return HttpResponse(status=200)

# Create your views here.
from django.shortcuts import render

from django.http import HttpResponse, HttpRequest
from django.conf import settings #noqa
from django.views.decorators.csrf import csrf_exempt
from withings import WithingsApi, WithingsCredentials
from withings_tasks import fetch_measurement

import json, logging, pprint

logger = logging.getLogger("medical_compliance.measurement_callback")

def get_full_callback_url(request):
    return request.build_absolute_uri("/notify_measurements/")

def subscribe_notifications(request):
    credentials = WithingsCredentials(access_token=settings.WITHINGS_OAUTH_V1_TOKEN,
                                      access_token_secret=settings.WITHINGS_OAUTH_V1_TOKEN_SECRET,
                                      consumer_key=settings.WITHINGS_CONSUMER_KEY,
                                      consumer_secret=settings.WITHINGS_CONSUMER_SECRET,
                                      user_id=settings.WITHINGS_USER_ID)
    client = WithingsApi(credentials)
    response_data = client.subscribe(get_full_callback_url(request), "Subscribe for weight measurement notifications.", appli=1)
    return HttpResponse(json.dumps(response_data), content_type="application/json")

def unsubscribe_notifications(request):
    credentials = WithingsCredentials(access_token=settings.WITHINGS_OAUTH_V1_TOKEN,
                                      access_token_secret=settings.WITHINGS_OAUTH_V1_TOKEN_SECRET,
                                      consumer_key=settings.WITHINGS_CONSUMER_KEY,
                                      consumer_secret=settings.WITHINGS_CONSUMER_SECRET,
                                      user_id=settings.WITHINGS_USER_ID)
    client = WithingsApi(credentials)
    response_data = client.unsubscribe(get_full_callback_url(request), appli=1)
    return HttpResponse(json.dumps(response_data), content_type="application/json")

@csrf_exempt
def notify_measurements(request):
    logger.debug(pprint.pformat(request.POST))
    
    if request.POST.has_key("userid"):
        userid = request.POST['userid']
        startdate = request.POST['startdate']
        enddate = request.POST['enddate']
        appli = request.POST['appli']
        fetch_measurement.delay(userid, startdate, enddate, appli)
    else:
        logger.debug(pprint.pformat("Received invalid post data; possibly related to a subscribe call"))

    return HttpResponse(status=200)

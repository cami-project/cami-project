# Create your views here.

import json, logging, pprint

from django.shortcuts import render
from django.http import HttpResponse, HttpRequest
from django.conf import settings #noqa
from django.views.decorators.csrf import csrf_exempt

from withings import WithingsApi, WithingsCredentials
from withings_tasks import save_measurement
from tasks import fetch_heart_rate_measurement


logger = logging.getLogger("medical_compliance.measurement_callback")


def get_full_callback_url(request):
    return request.build_absolute_uri("/measurements_notification/")

def subscribe_notifications(request):
    logger.debug("[medical-compliance] Notifications subscribe was called.")
    credentials = WithingsCredentials(access_token=settings.WITHINGS_OAUTH_V1_TOKEN,
                                      access_token_secret=settings.WITHINGS_OAUTH_V1_TOKEN_SECRET,
                                      consumer_key=settings.WITHINGS_CONSUMER_KEY,
                                      consumer_secret=settings.WITHINGS_CONSUMER_SECRET,
                                      user_id=settings.WITHINGS_USER_ID)
    client = WithingsApi(credentials)
    response_data = client.subscribe(get_full_callback_url(request), "Subscribe for weight measurement notifications.", appli=1)
    logger.debug("[medical-compliance] Notifications subscribe response: %s." % (response_data))

    return HttpResponse(json.dumps(response_data), content_type="application/json")

def unsubscribe_notifications(request):
    logger.debug("[medical-compliance] Notifications unsubscribe was called.")
    credentials = WithingsCredentials(access_token=settings.WITHINGS_OAUTH_V1_TOKEN,
                                      access_token_secret=settings.WITHINGS_OAUTH_V1_TOKEN_SECRET,
                                      consumer_key=settings.WITHINGS_CONSUMER_KEY,
                                      consumer_secret=settings.WITHINGS_CONSUMER_SECRET,
                                      user_id=settings.WITHINGS_USER_ID)
    client = WithingsApi(credentials)
    response_data = client.unsubscribe(get_full_callback_url(request), appli=1)    
    logger.debug("[medical-compliance] Notifications unsubscribe response: %s." % (response_data))

    return HttpResponse(json.dumps(response_data), content_type="application/json")

def test_heart_rate_fetch(request):
    return HttpResponse(fetch_heart_rate_measurement(), content_type="text/html")

@csrf_exempt
def measurements_notification_received(request):
    logger.debug("[medical-compliance] Measurements hook was called: %s" % (request.POST))
    
    if request.POST.has_key("userid"):
        userid = request.POST['userid']
        startdate = request.POST['startdate']
        enddate = request.POST['enddate']
        appli = request.POST['appli']

        logger.debug("[medical-compliance] Passing data to the save_measurement task: { userid: %s, startdate: %s, enddate: %s, appli: %s }" %
            (userid, startdate, enddate, appli))
        save_measurement.delay(userid, startdate, enddate, appli)
    else:
        logger.debug("[medical-compliance] Received invalid POST data in measurements hook - possibly related to a subscribe call")

    return HttpResponse(status=200)
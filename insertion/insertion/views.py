import json, httplib
import logging
import settings
import requests

from kombu import Connection, Producer

from django.http import HttpResponse
from django.views.decorators.csrf import csrf_exempt
from . import librato_api, USER_ENVIRONMENT, USER_NOTIFICATIONS

logger = logging.getLogger("insertion")


"""
================ Auxiliary functions ================
"""
def _get_id_from_uri_path(uriPath):
    res = uriPath.rstrip("/")
    return int(res.split("/")[-1])


def _get_user_for_gateway(gatewayURIPath):
    endpoint = settings.STORE_ENDPOINT_URI + gatewayURIPath
    r = requests.get(endpoint)

    response_json = r.json()
    return r.status_code, response_json

def _get_user_data(userURIPath):
    endpoint = settings.STORE_ENDPOINT_URI + userURIPath
    r = requests.get(endpoint)

    response_json = r.json()
    return r.status_code, response_json


"""
============================ Librato metrics ============================
"""
def log_environment_event_metrics(event_data):
    if event_data["content"]["name"] == "presence" and\
            event_data["content"]["value"]["alarm_motion"] == True:

        logger.info("[insertion] Logging motion activation metrics for event: %s" % event_data)

        event_metric_tags = {
            "gateway": event_data["annotations"]["source"]["gateway"]
        }

        status, gateway_data = _get_user_for_gateway(event_data["annotations"]["source"]["gateway"])
        if status == httplib.OK:
            user_uri_path = gateway_data["user"]
            event_metric_tags["user"] = user_uri_path

            s, d = _get_user_data(user_uri_path)
            if s == httplib.OK:
                event_metric_tags["username"] = d["username"]
            else:
                logger.error("[insertion] Could not retrieve data from CAMI Store for user: %s" +
                             ". Reason: %s" % user_uri_path , d)
        else:
            logger.error("[insertion] Could not retrieve data from CAMI Store for gateway: "
                         + event_data["annotations"]["source"]["gateway"] + ". Reason: %s" % gateway_data)

        logger.info("[insertion] Submitting motion_triggered metric to Librato with the following tags: %s" % event_metric_tags)
        librato_api.submit("cami.motion.triggered", 1, type="counter", tags = event_metric_tags)



def log_notifications_event_metrics(event_data):
    pass


"""
======================== INSERTION VIEWS ========================
"""

@csrf_exempt
def insert_measurement(request):
    if request.method != "POST":
        return HttpResponse(status=405)

    try:
        content = json.loads(request.body)
        if 'measurement_type' in content:
            # get a connection to RabbitMQ broker, create a channel and create a
            # producer for pushing the message to the measurements exchange
            with Connection(settings.BROKER_URL) as conn:
                channel = conn.channel()

                inserter = Producer(
                    exchange=settings.MEASUREMENTS_EXCHANGE,
                    channel=channel,
                    routing_key="measurement." + content['measurement_type']
                )
                inserter.publish(request.body)

                logger.debug("[insertion] New measurement was enqueued: %s", str(content))

                return HttpResponse(status=201)
    except Exception as e:
        logger.debug("[insertion] ERROR! Exception caught in insert_measurement method: %s", e.message)

    return HttpResponse(status=400)

@csrf_exempt
def insert_event(request):
    if request.method != "POST":
        return HttpResponse(status=405)

    try:
        content = json.loads(request.body)
        if 'category' in content:

            # get a connection to RabbitMQ broker, create a channel and create a producer for pushing the message to the appropriate CAMI event exchange
            try:
                conn = Connection(settings.BROKER_URL)
                channel = conn.channel()

                inserter = Producer(
                    exchange=settings.EVENTS_EXCHANGE,
                    channel=channel,
                    routing_key="event." + content['category']
                )
                inserter.publish(request.body)

                logger.debug("[insertion] New event was enqueued: %s", str(content))
            except Exception as ex:
                logger.error("[insertion] Error inserting event %s into RabbitMQ exchange: %s", content, ex)
                return HttpResponse(json.dumps({"error": "Could not insert event %s to RabbitMQ events exchange" % content}),
                                    status=500, content_type="application/json")

            # log into librato
            logger.info("[insertion] Preparing to send librato metrics for event: %s" % content)

            ## filter by the category of user event
            try:
                event_category = content["category"].lower()
                if event_category == USER_ENVIRONMENT:
                    log_environment_event_metrics(content)
                elif event_category == USER_NOTIFICATIONS:
                    log_notifications_event_metrics(content)
            except Exception as ex:
                logger.error("[insertion] Error! Exception thrown when logging event metric. Reason: %s" % ex)

            return HttpResponse(status=201)
    except Exception as e:
        logger.debug("[insertion] ERROR! Exception caught in insert_event method: %s", e.message)

    return HttpResponse(status=400)


@csrf_exempt
def insert_push_notification(request):
    if request.method != "POST":
        return HttpResponse(status=405)

    try:
        content = json.loads(request.body)

        if 'user_id' in content and 'message' in content:
            # get a connection to RabbitMQ broker, create a channel and create a producer for pushing the message to the appropriate CAMI event exchange
            with Connection(settings.BROKER_URL) as conn:
                channel = conn.channel()

                inserter = Producer(
                    exchange=settings.PUSH_NOTIFICATIONS_EXCHANGE,
                    channel=channel,
                    routing_key="push_notification"
                )
                inserter.publish(request.body)

                logger.debug("[insertion] New push notification was enqueued: %s", str(content))

                return HttpResponse(status=201)
    except Exception as e:
        logger.debug("[insertion] ERROR! Exception caught in insert_push_notification method: %s", e.message)

    return HttpResponse(status=400)





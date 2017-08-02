import json
import logging
import settings

from kombu import Connection, Producer

from django.http import HttpResponse
from django.views.decorators.csrf import csrf_exempt


logger = logging.getLogger("insertion")

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
    except Exception:
        logger.debug("[insertion] ERROR! Exception caught in insert_measurement method: %s", e.value)

    return HttpResponse(status=400)

@csrf_exempt
def insert_event(request):
    if request.method != "POST":
        return HttpResponse(status=405)

    try:
        content = json.loads(request.body)
        if 'category' in content:
            # get a connection to RabbitMQ broker, create a channel and create a producer for pushing the message to the appropriate CAMI event exchange
            with Connection(settings.BROKER_URL) as conn:
                channel = conn.channel()

                inserter = Producer(
                    exchange=settings.EVENTS_EXCHANGE,
                    channel=channel,
                    routing_key="event." + content['category']
                )
                inserter.publish(request.body)

                logger.debug("[insertion] New event was enqueued: %s", str(content))

                return HttpResponse(status=201)
    except Exception:
        logger.debug("[insertion] ERROR! Exception caught in insert_event method: %s", e.value)

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
        logger.debug("[insertion] ERROR! Exception caught in insert_push_notification method: %s", e.value)

    return HttpResponse(status=400)

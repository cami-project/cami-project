from django.http import HttpResponse
from django.views.decorators.csrf import csrf_exempt
from kombu import Connection, Producer

import settings
import logging, json


measurement_logger = logging.getLogger("insertion.measurements")
events_logger = logging.getLogger("insertion.events")

ENVIRONMENT_EVENT = "USER_ENVIRONMENT"
USER_HEALTH = "USER_HEALTH"
USER_NOTIFICATIONS = "USER_NOTIFICATIONS"

@csrf_exempt
def insert_measurement(request):
    content = json.loads(request.body)
    if 'measurement_type' in content:
        # get a connection to RabbitMQ broker, create a channel and create a producer for pushing the message to the health_measurements exchange
        with Connection(settings.BROKER_URL) as conn:
            channel = conn.channel()

            inserter = Producer(exchange = settings.HEALTH_MEASUREMENTS_EXCHANGE, channel=channel, routing_key="health_measurements")
            inserter.publish(request.body)

            return HttpResponse(status = 201)

    return HttpResponse(status = 405)

@csrf_exempt
def insert_event(request):
    content = json.loads(request.body)
    if 'category' in content:
        # get a connection to RabbitMQ broker, create a channel and create a producer for pushing the message to the appropriate CAMI event exchange
        with Connection(settings.BROKER_URL) as conn:
            channel = conn.channel()

            if content['category'] == ENVIRONMENT_EVENT:
                inserter = Producer(exchange=settings.ENVIRONMENT_EVENTS_EXCHANGE, channel=channel,
                                    routing_key="events.environment")
                inserter.publish(request.body)

            elif content['category'] ==  USER_HEALTH:
                inserter = Producer(exchange=settings.HEALTH_EVENTS_EXCHANGE, channel=channel,
                                    routing_key="events.health")
                inserter.publish(request.body)

            elif content['category'] == USER_NOTIFICATIONS:
                inserter = Producer(exchange=settings.HEALTH_EVENTS_EXCHANGE, channel=channel,
                                    routing_key="events.notification")
                inserter.publish(request.body)

            return HttpResponse(status = 201)

    return HttpResponse(status = 405)

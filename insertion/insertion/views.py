from django.http import HttpResponse
from django.conf import settings
from kombu import Connection, Producer

import logging, json


measurement_logger = logging.getLogger("insertion.measurements")
events_logger = logging.getLogger("insertion.events")

def insert_measurement(request):
    content = json.loads(request.body)
    if 'measurement_type' in content:
        # get a connection to RabbitMQ broker, create a channel and create a producer for pushing the message to the health_measurements exchange
        with Connection(settings.BROKER_URL) as conn:
            channel = conn.channel()

            inserter = Producer(exchange = settings.health_measurements_exchange, channel=channel, routing_key="health_measurements")
            inserter.publish(request.body)

            return HttpResponse(status = 201)

    return HttpResponse(status = 405)


def insert_event(request):
    return HttpResponse(status = 201)


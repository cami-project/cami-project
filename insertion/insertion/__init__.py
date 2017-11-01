from __future__ import absolute_import

from . import settings
from kombu import Connection
import pika, json, httplib
import logging
import librato

USER_ENVIRONMENT = "user_environment"
USER_HEALTH = "user_health"
USER_NOTIFICATIONS = "user_notifications"

logger = logging.getLogger("insertion")

# When Django starts, we want to have already declared the exchanges for each type of measurement or event
with Connection(settings.BROKER_URL) as conn:
    channel = conn.channel()

    for exchange in settings.BROKER_EXCHANGES:
        # bind the exchange to the channel
        bound_exchange = exchange(channel)

        # declare the exchange
        bound_exchange.declare()

"""
Also, we want to setup the Librato metrics monitoring API.
So, upon startup, we will declare two monitoring queues, one for measurements and one for events, in the style
of the `consumer.py script`
"""
librato_api = librato.connect(settings.LIBRATO_EMAIL, settings.LIBRATO_TOKEN, sanitizer=librato.sanitize_metric_name)

parameters = pika.URLParameters(settings.BROKER_URL)
connection = pika.BlockingConnection(parameters)
channel = connection.channel()

# Create a temporary queue only for receiving data from the exchange
result = channel.queue_declare(queue = "librato-event-monitoring", exclusive=True)
event_queue_name = result.method.queue

result = channel.queue_declare(queue = "librato-measurement-monitoring", exclusive=True)
measurement_queue_name = result.method.queue


"""
================ Auxiliary functions ================
"""
def _get_id_from_uri_path(uriPath):
    res = uriPath.rstrip("/")
    return int(res.split("/")[-1])


def _get_user_for_gateway(gatewayURIPath):
    import requests

    endpoint = settings.STORE_ENDPOINT_URI + gatewayURIPath
    r = requests.get(endpoint)

    response_json = r.json()
    return r.status_code, response_json

def _get_user_data(userURIPath):
    import requests

    endpoint = settings.STORE_ENDPOINT_URI + userURIPath
    r = requests.get(endpoint)

    response_json = r.json()
    return r.status_code, response_json

"""
================ Metric logging functions ================
"""
def event_metrics_callback(ch, method, properties, body):
    #print(" [x] %r" % body)
    logger.info("[insertion] Callback for RabbitMQ monitoring queue on channel %s with body %s" % ch, body)
    event_data = json.loads(body)

    logger.info("[insertion] Preparing to send librato metrics for event: %s" % event_data)

    ## filter by the category of user event
    event_category = event_data["category"].lower()
    if event_category == USER_ENVIRONMENT:
        log_environment_event_metrics(event_data)
    elif event_category == USER_NOTIFICATIONS:
        log_notifications_event_metrics(event_data)


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
                logger.error("[insertion] Could not retrieve data from CAMI Store for user: " + user_uri_path +
                             ". Reason: " + str(d))
        else:
            logger.error("[insertion] Could not retrieve data from CAMI Store for gateway: "
                         + event_data["annotations"]["source"]["gateway"] + ". Reason: " + str(gateway_data))

        logger.info("[insertion] Submitting motion_triggered metric to Librato with the following tags: %s" % event_metric_tags)
        librato_api.submit("motion_triggered", 1, event_metric_tags)



def log_notifications_event_metrics(event_data):
    pass


def measurement_metrics_callback(ch, method, properties, body):
    pass


"""
================ Binding to RabbitMQ ================
"""
## bind event monitoring queue
channel.queue_bind(
	# We want to monitor 'event' data, so we bind with
	# the 'events' exchange
    exchange='events',
    queue=event_queue_name,
    # The 'events' exhange is of `topic` type
    # We can get the messages using wildcards
    # Using this routing key, we'll basically receive all
    # the events, no matter their type
    routing_key='event.*'
)

## bind measuement monitoring queue
channel.queue_bind(
	# We want to monitor 'event' data, so we bind with
	# the 'measurements' exchange
    exchange='measurements',
    queue=measurement_queue_name,
    # The 'measurements' exhange is of `topic` type
    # We can get the messages using wildcards
    # Using this routing key, we'll basically receive all
    # the measurements, no matter their type
    routing_key='measurement.*'
)

## start the actual consumption
channel.basic_consume(
    event_metrics_callback,
    queue=event_queue_name,
    no_ack=True,
    consumer_tag="event-metrics-consumer"
)

channel.basic_consume(
    measurement_metrics_callback,
    queue=measurement_queue_name,
    no_ack=True,
    consumer_tag="measurement-metrics-consumer"
)

try:
    channel.start_consuming()
except Exception as ex:
    logger.error("[insertion] Error recording event or measurement metrics: " + ex)
    #channel.stop_consuming()
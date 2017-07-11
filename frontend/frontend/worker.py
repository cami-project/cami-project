import json
import logging
import logging.config

from kombu import Queue, Exchange, Connection
from kombu.mixins import ConsumerMixin

from django.conf import settings

# Local imports
from frontend import store_utils
from frontend.push_notifications import notifications


logging.config.dictConfig(settings.LOGGING)
logger = logging.getLogger("frontend.worker")

class Worker(ConsumerMixin):
    def __init__(self, connection, queues):
        self.connection = connection
        self.queues = queues

    def get_consumers(self, Consumer, channel):
        return [
            Consumer(
                queues=self.queues,
                callbacks=[self.on_message],
                accept=['json']
            )
        ]

    def on_message(self, body, message):
        payload = json.loads(body)

        logger.debug("[frontend] Send notification request: %s" % (locals()))

        devices = store_utils.pushnotificationdevice_get(user=int(payload['user_id']))
        if devices:
            notifications.send_message(devices, payload['message'], sound="default")

        message.ack()

with Connection(settings.BROKER_URL, heartbeat=4) as conn:
    worker = Worker(conn, settings.QUEUES)
    logger.debug("[frontend] Worker is starting...")
    worker.run()

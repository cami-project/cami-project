from celery import Celery
from celery.utils.log import get_task_logger

from kombu import Queue, Exchange
from kombu.common import Broadcast

import endpoints
import sys

from custom_logging import logger
from settings import BROKER_URL, BROKER_QUEUE, BROKER_TASK

app = Celery('api.tasks', broker=BROKER_URL)
app.conf.update(
    CELERY_DEFAULT_QUEUE=BROKER_QUEUE,
    CELERY_QUEUES=(
        Broadcast(BROKER_QUEUE),
    ),
)

@app.task(name=BROKER_TASK)
def on_measurement_received(measurement_json):
    logger.debug('[opentele] Measurement received: %s' % (measurement_json))

    try:
        enpoints.process_measurement(measurement_json)
    except Exception, e:
        logger.error('[opentele] Error processing measurement: %s' % (e))
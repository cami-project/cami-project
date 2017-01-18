from celery import Celery
from kombu import Queue, Exchange
from kombu.common import Broadcast

import endpoints
from settings import *

import logging
from logging.handlers import SysLogHandler
import sys

root = logging.getLogger()
root.setLevel(logging.DEBUG)

formatter = logging.Formatter('%(asctime)s - %(name)s - %(levelname)s - %(message)s')

stdout = logging.StreamHandler(sys.stdout)
stdout.setFormatter(formatter)

syslog = SysLogHandler(address=(PAPERTRAILS_LOGGING_HOSTNAME, PAPERTRAILS_LOGGING_PORT))
syslog.setFormatter(formatter)

root.addHandler(stdout)
root.addHandler(syslog)


app = Celery('api.tasks', broker=BROKER_URL)
app.conf.update(
    CELERY_DEFAULT_QUEUE=BROKER_QUEUE,
    CELERY_QUEUES=(
        Broadcast(BROKER_QUEUE),
    ),
)

@app.task(name=BROKER_TASK)
def parse_measurement(measurement_json):
    root.debug('[linkwatch] Parse measurement request: %s' % (measurement_json))
    # endpoints.save_measurement(measurement_json)
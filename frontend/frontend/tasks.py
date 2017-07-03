"""
Define tasks that can be sent by other services through the RabbitMQ broker.

All tasks have manually defined names instead of automatic[1] to eliminate the
need to have the same module structure in the worker and the client.

[1] http://docs.celeryproject.org/en/latest/userguide/tasks.html#task-naming-relative-imports
"""
from __future__ import absolute_import

import time

from kombu import Queue, Exchange
from celery import Celery
from celery.utils.log import get_task_logger

from django.conf import settings

# Local imports
from frontend import store_utils
from frontend.push_notifications import notifications


logger = get_task_logger('frontend.tasks')

app = Celery('api.tasks', broker=settings.BROKER_URL)
app.conf.update(
    CELERY_DEFAULT_QUEUE='frontend_notifications',
    CELERY_QUEUES=(
        Queue('frontend_notifications', Exchange('frontend_notifications'), routing_key='frontend_notifications'),
    ),
)

@app.task(name='frontend.send_notification')
def send_notification(user_id, message):
    logger.debug("[frontend] Send notification request: %s" % (locals()))
    devices = store_utils.pushnotificationdevice_get(user=int(user_id))
    if devices:
    	notifications.send_message(devices, message, sound="default")

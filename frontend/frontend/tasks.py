"""
Define tasks that can be sent by other services through the RabbitMQ broker.

All tasks have manually defined names instead of automatic[1] to eliminate the
need to have the same module structure in the worker and the client.

[1] http://docs.celeryproject.org/en/latest/userguide/tasks.html#task-naming-relative-imports
"""

import time
import celery

from kombu import Queue, Exchange
from celery import Celery
from celery.utils.log import get_task_logger

from django.conf import settings


logger = get_task_logger('frontend.tasks')

app = Celery('api.tasks', broker=settings.BROKER_URL)
app.conf.update(
    CELERY_DEFAULT_QUEUE='frontend_notifications',
    CELERY_QUEUES=(
        Queue('frontend_notifications', Exchange('frontend_notifications'), routing_key='frontend_notifications'),
    ),
)

@celery.task(name='frontend.send_notification')
def send_notification(user_id, recipient_type, type, severity, message, description, timestamp):
    logger.debug("[frontend] Send notification request: %s" % (locals()))

    if timestamp is None:
        timestamp = time.time()
        
    """
    n = Notification(user_id=user_id, recipient_type=recipient_type, type=type, severity=severity, timestamp=timestamp, message=message, description=description)
    n.full_clean()
    n.save()

    devices = APNSDevice.objects.filter(name=recipient_type)
    devices.send_message(message, sound="default")
    """
    
    logger.debug("[frontend] Notifications were successfully sent.")
"""
Define tasks that can be sent by other services through the RabbitMQ broker.

All tasks have manually defined names instead of automatic[1] to eliminate the
need to have the same module structure in the worker and the client.

[1] http://docs.celeryproject.org/en/latest/userguide/tasks.html#task-naming-relative-imports
"""

import celery

from celery.utils.log import get_task_logger

from models import Notification


logger = get_task_logger(__name__)


@celery.task(name='frontend.send_notification')
def send_notification(message, type, severity):
    n = Notification(message=message, type=type, severity=severity)
    n.save()

    logger.debug(
        "Received and saved notification: %s %s %s",
        message,
        type,
        severity
    )

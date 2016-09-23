"""
Define tasks that can be sent by other services through the RabbitMQ broker.
All tasks have manually defined names instead of automatic[1] to eliminate the
need to have the same module structure in the worker and the client.
[1] http://docs.celeryproject.org/en/latest/userguide/tasks.html#task-naming-relative-imports
"""

import celery

from celery import Celery
from celery.utils.log import get_task_logger
from kombu import Queue, Exchange
from django.conf import settings #noqa

from models import WeightMeasurement
from analyzers.weight_analyzers import analyze_weights

logger = get_task_logger("medical_compliance_measurements.fetch_weight_measurement")


app = Celery('api.tasks', broker=settings.BROKER_URL)
app.conf.update(
    CELERY_DEFAULT_QUEUE='medical_compliance_measurements',
    CELERY_QUEUES=(
        Queue('medical_compliance_measurements', Exchange('medical_compliance_measurements'), routing_key='medical_compliance_measurements'),
    ),
)


@app.task(name='medical_compliance_measurements.fetch_weight_measurement')
def fetch_weight_measurement(user_id, input_source, measurement_unit, timestamp, timezone, value):
    logger.debug(
        "Received weight measurement for user_id: %s, input_source: %s",
        str(user_id),
        str(input_source)
    )

    weightMeas = WeightMeasurement(
            user_id = int(user_id),
            input_source=input_source,
            measurement_unit=measurement_unit,
            timestamp=timestamp,
            timezone=timezone,
            value=value)
    weightMeas.save()
    analyze_weights.delay(weightMeas.id)
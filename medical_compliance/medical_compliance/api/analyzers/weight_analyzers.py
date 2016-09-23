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

from ..models import WeightMeasurement

logger = get_task_logger("medical_compliance_weight_analyzers.analyze_weight")


app = Celery('api.tasks', broker=settings.BROKER_URL)
app.conf.update(
    CELERY_DEFAULT_QUEUE='medical_compliance_weight_analyzers',
    CELERY_QUEUES=(
        Queue('medical_compliance_weight_analyzers', Exchange('medical_compliance_weight_analyzers'), routing_key='medical_compliance_weight_analyzers'),
    ),
)

@app.task(name='medical_compliance_weight_analyzers.analyze_weights')
def analyze_weights(weight_measurement_id):
    DELTA_KG = 2
    logger.debug(
        "Received weight_measurement_id: %s",
        str(weight_measurement_id)
    )
    analyze_last_two_weights(weight_measurement_id)


def analyze_last_two_weights(weight_measurement_id):
    measurement_list = WeightMeasurement.get_previous_weight_measures(weight_measurement_id, 2)
    if len(measurement_list) == 2:
        currentMeasurement = measurement_list[0]
        previousMeasurement = measurement_list[1]
        
        # TODO: analyze currentMeasurement vs previousMeasurement and generate notification
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
    analyze_last_two_weights(weight_measurement_id)

# TODO: this is a dummy module and should be generalized at least with a task structure
# all the tasks should listen on the same weight queue and all of them should compute some metrics (broadcast?)
def analyze_last_two_weights(weight_measurement_id):
    measurement_list = WeightMeasurement.get_previous_weight_measures(weight_measurement_id, 2)
    if len(measurement_list) == 2:
        current_measurement= measurement_list[0]
        previous_measurement = measurement_list[1]
        
        delta_value = current_measurement.value - previous_measurement.value

        if delta_value <= -2:
            message = u"Jim lost %s kg" % (abs(delta_value))
            description = "You can contact him and see what's wrong."
            send_notification(current_measurement.user_id, "warning", message, description)
        if delta_value >= 2:
            message = u"Jim gained %s kg" % (abs(delta_value))
            description = "Please check if this has to do with his diet."
            send_notification(current_measurement.user_id, "warning", message, description)

def send_notification(user_id, status, message, description):
    app = Celery()
    app.config_from_object('django.conf:settings')
    app.send_task('frontend.send_notification', (user_id, 'weight', status, message, description), queue='frontend_notifications')
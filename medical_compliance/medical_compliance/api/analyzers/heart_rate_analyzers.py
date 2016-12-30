"""
Define tasks that can be sent by other services through the RabbitMQ broker.
All tasks have manually defined names instead of automatic[1] to eliminate the
need to have the same module structure in the worker and the client.
[1] http://docs.celeryproject.org/en/latest/userguide/tasks.html#task-naming-relative-imports
"""

import celery
import numpy as np

from datetime import datetime
from celery import Celery
from celery.utils.log import get_task_logger
from kombu import Queue, Exchange
from django.conf import settings #noqa
from notifications_adapter import NotificationsAdapter

from ..models import HeartRateMeasurement

logger = get_task_logger("medical_compliance_heart_rate_analyzers.analyze_heart_rates")


app = Celery('api.tasks', broker=settings.BROKER_URL)
app.conf.update(
    CELERY_DEFAULT_QUEUE='medical_compliance_heart_rate_analyzers',
    CELERY_QUEUES=(
        Queue('medical_compliance_heart_rate_analyzers', Exchange('medical_compliance_heart_rate_analyzers'), routing_key='medical_compliance_heart_rate_analyzers'),
    ),
)

@app.task(name='medical_compliance_heart_rate_analyzers.analyze_heart_rates')
def analyze_heart_rates(last_measurement, input_source):
    analyze_last_heart_rates(last_measurement, input_source)

# TODO: this is a dummy module and should be generalized at least with a task structure
# all the tasks should listen on the same heart rate queue and all of them should compute some metrics (broadcast?)
def analyze_last_heart_rates(last_measurement, input_source):
    last_timestamp = 0

    if last_measurement:
        last_timestamp = last_measurement.timestamp

    measurement_list = HeartRateMeasurement.objects.filter(
        timestamp__gt=last_timestamp
    ).filter(input_source=input_source).order_by('timestamp')

    notifications_adapter = NotificationsAdapter()

    for m in measurement_list:
        if m.value < 60:
            message = u"Jim's heart rate is dangerously low: only %d." % int(m.value)
            description = "Please take action now!"
            notifications_adapter.send_caregiver_notification(measurement_list[0].user_id, "heart", "high", message, description, m.timestamp)

            message = u"Hey Jim! Your heart rate is quite low: %d." % int(m.value)
            description = "I have contacted your caregiver."
            notifications_adapter.send_elderly_notification(measurement_list[0].user_id, "heart", "high", message, description, m.timestamp)
        elif m.value < 70:
            message = u"Jim's heart rate is a bit low: only %d" % int(m.value)
            description = "Please make sure he's all right."
            notifications_adapter.send_caregiver_notification(measurement_list[0].user_id, "heart", "medium", message, description, m.timestamp)

            message = u"Hey Jim! Your heart rate is just a bit low: %d." % int(m.value)
            description = "How about some exercise?"
            notifications_adapter.send_elderly_notification(measurement_list[0].user_id, "heart", "medium", message, description, m.timestamp)

        if m.value > 100:
            message = u"Jim's heart rate is dangerously high: over %d." % int(m.value)
            description = "Please take action now!"
            notifications_adapter.send_caregiver_notification(measurement_list[0].user_id, "heart", "high", message, description, m.timestamp)

            message = u"Hey Jim! Your heart rate is quite high: %d." % int(m.value)
            description = "I have contacted your caregiver."
            notifications_adapter.send_elderly_notification(measurement_list[0].user_id, "heart", "high", message, description, m.timestamp)

        elif m.value > 85:
            message = u"Jim's heart rate is a bit high: over %d." % int(m.value)
            description = "Please make sure he's alright."
            notifications_adapter.send_caregiver_notification(measurement_list[0].user_id, "heart", "medium", message, description, m.timestamp)

            message = u"Hey Jim! Your heart rate is just a bit high: %d." % int(m.value)
            description = "Why not rest for a bit?"
            notifications_adapter.send_elderly_notification(measurement_list[0].user_id, "heart", "medium", message, description, m.timestamp)
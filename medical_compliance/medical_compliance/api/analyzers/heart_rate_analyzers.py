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
def analyze_heart_rates():
    analyze_last_heart_rates()

# TODO: this is a dummy module and should be generalized at least with a task structure
# all the tasks should listen on the same heart rate queue and all of them should compute some metrics (broadcast?)
def write_last_notified_hrm_timestamp(timestamp):
    f = open("last_timestamp", "w")
    f.write(str(timestamp))
    f.close()

def read_last_notified_hrm_timestamp():
    f = open("last_timestamp", "r")
    timestamp = int(f.read())
    f.close()
    return timestamp

def analyze_last_heart_rates():
    notifications_adapter = NotificationsAdapter()

    last_timestamp = read_last_notified_hrm_timestamp()
    measurement_list = HeartRateMeasurement.objects.filter(
        timestamp__gt=last_timestamp
    ).order_by('-timestamp')

    values = []
    timestamps = []
    window_timestamp = measurement_list[len(measurement_list) - 1].timestamp + 1200

    for m in reversed(measurement_list):
        if m.timestamp <= window_timestamp and m is not measurement_list[0]:
            values.append(m.value)
            timestamps.append(m.timestamp)
        else:
            if m is measurement_list[0] and m.timestamp <= window_timestamp:
                values.append(m.value)
                timestamps.append(m.timestamp)
            
            if len(values) < 3:
                values = [m.value]
                timestamps = [m.timestamp]
                window_timestamp = window_timestamp + 1200
                continue

            mean_val = np.mean(values)
            time_from = datetime.fromtimestamp(
                timestamps[0]
            ).strftime('%H:%M')
            time_to = datetime.fromtimestamp(
                timestamps[-1]
            ).strftime('%H:%M')

            sent_notification = False

            if mean_val < 60:
                message = u"Jim had a low heart rate: %d between %s and %s" % \
                    (int(mean_val), time_from, time_to)
                description = "You can contact him and see what was wrong."
                notifications_adapter.send_caregiver_notification(measurement_list[0].user_id, "heart", "medium", message, description)

                message = u"You had a low heart rate: %d between %s and %s" % \
                    (int(mean_val), time_from, time_to)
                description = "Please take care."
                notifications_adapter.send_elderly_notification(measurement_list[0].user_id, "heart", "medium", message, description)
                sent_notification = True

            if mean_val > 100:
                message = u"Jim had a high heart rate: %d between %s and %s" % \
                    (int(mean_val), time_from, time_to)
                description = "You can contact him and see what was wrong."
                notifications_adapter.send_caregiver_notification(measurement_list[0].user_id, "heart", "medium", message, description)

                message = u"You had a high heart rate: %d between %s and %s" % \
                    (int(mean_val), time_from, time_to)
                description = "Please take care."
                notifications_adapter.send_elderly_notification(measurement_list[0].user_id, "heart", "medium", message, description)
                sent_notification = True

            if sent_notification:
                last_timestamp = timestamps[-1]

            values = [m.value]
            timestamps = [m.timestamp]
            window_timestamp = window_timestamp + 1200

    write_last_notified_hrm_timestamp(last_timestamp)
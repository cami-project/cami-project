"""
Define tasks that can be sent by other services through the RabbitMQ broker.
All tasks have manually defined names instead of automatic[1] to eliminate the
need to have the same module structure in the worker and the client.
[1] http://docs.celeryproject.org/en/latest/userguide/tasks.html#task-naming-relative-imports
"""

import json
import datetime

from celery import Celery
from celery.utils.log import get_task_logger
from kombu import Queue, Exchange
from django.conf import settings #noqa

import google_fit

from models import WeightMeasurement, HeartRateMeasurement

from analyzers.weight_analyzers import analyze_weights
from analyzers.heart_rate_analyzers import analyze_heart_rates


app = Celery('api.tasks', broker=settings.BROKER_URL)
app.conf.update(
    CELERY_DEFAULT_QUEUE='medical_compliance_measurements',
    CELERY_QUEUES=(
        Queue('medical_compliance_measurements', Exchange('medical_compliance_measurements'), routing_key='medical_compliance_measurements'),
    ),
)


@app.task(name='medical_compliance_measurements.fetch_weight_measurement')
def fetch_weight_measurement(user_id, input_source, measurement_unit, timestamp, timezone, value):
    weight_measurement = WeightMeasurement(
        user_id = int(user_id),
        input_source=input_source,
        measurement_unit=measurement_unit,
        timestamp=timestamp,
        timezone=timezone,
        value=value
    )
    weight_measurement.save()
    analyze_weights.delay(weight_measurement.id)

@app.task(name='medical_compliance_measurements.fetch_heart_rate_measurement')
def fetch_heart_rate_measurement():
    if HeartRateMeasurement.objects.count() > 0:
        last_meas = HeartRateMeasurement.objects.all().order_by('-timestamp')[0]
        time_from = str(last_meas.timestamp + 1) + '000000000'
    else:
        time_from = str(0)

    time_to = str(
        int(
            (
                datetime.datetime.today() + 
                datetime.timedelta(days=1) - 
                datetime.datetime(1970, 1, 1)
            ).total_seconds()
        )
    ) + '000000000'
    
    measurements = google_fit.get_heart_rate_data(time_from, time_to)

    for m in measurements:
        heart_rate_measurement = HeartRateMeasurement(
            user_id = 11262861,
            input_source='google_fit',
            measurement_unit='bpm',
            timestamp=m['timestamp'],
            timezone='Europe/Bucharest',
            value=m['value']
        )
        heart_rate_measurement.save()

    analyze_heart_rates.delay(last_meas)

    return json.dumps(measurements, indent=4, sort_keys=True)
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

from models import WeightMeasurement, HeartRateMeasurement, StepsMeasurement

from analyzers.weight_analyzers import analyze_weights
from analyzers.heart_rate_analyzers import analyze_heart_rates

logger = get_task_logger('medical_compliance_measurements.process_measurement')

global_app = Celery()
global_app.config_from_object('django.conf:settings')

app = Celery('api.tasks', broker=settings.BROKER_URL)
app.conf.update(
    CELERY_DEFAULT_QUEUE='medical_compliance_measurements',
    CELERY_QUEUES=(
        Queue('medical_compliance_measurements', Exchange('medical_compliance_measurements'), routing_key='medical_compliance_measurements'),
    ),
)


@app.task(name='medical_compliance_measurements.process_weight_measurement')
def process_weight_measurement(user_id, input_source, measurement_unit, timestamp, timezone, value):
    logger.debug("[medical-compliance] Process weight measurement: %s" % (locals()))

    weight_measurement = WeightMeasurement(
        user_id = int(user_id),
        input_source=input_source,
        measurement_unit=measurement_unit,
        timestamp=timestamp,
        timezone=timezone,
        value=value
    )

    logger.debug("[medical-compliance] Saving weight measurement: %s" % (weight_measurement))
    weight_measurement.save()

    logger.debug("[medical-compliance] Sending the weight measurement with id %s for analysis." % (weight_measurement.id))
    analyze_weights.delay(weight_measurement.id)

    logger.debug("[medical-compliance] Broadcasting weight measurement: %s" % (weight_measurement))
    broadcast_measurement('weight', weight_measurement)

@app.task(name='medical_compliance_measurements.process_heart_rate_measurement')
def process_heart_rate_measurement():
    """
        It's very ugly what I did here, only for demo purpose
        We MUST clean this
    """
    logger.debug("[medical-compliance] Process heart measurement: %s. Retrieving test and cinch heart rate measurements since the last one..." % (locals()))

    last_cinch_measurement = None
    last_test_measurement = None
    
    cinch_heart_rate_measurements = HeartRateMeasurement.objects.all().filter(input_source='cinch').order_by('-timestamp')
    logger.debug("[medical-compliance] All cinch_heart_rate_measurements: " % (cinch_heart_rate_measurements))

    test_heart_rate_measurements = HeartRateMeasurement.objects.all().filter(input_source='test').order_by('-timestamp')
    logger.debug("[medical-compliance] All test_heart_rate_measurements: " % (test_heart_rate_measurements))

    try:
        last_cinch_measurement = HeartRateMeasurement.objects.all().filter(input_source='cinch').order_by('-timestamp')[0]
        time_from_cinch = last_cinch_measurement.timestamp + 1
    except:
        time_from_cinch = 0

    try:
        last_test_measurement = HeartRateMeasurement.objects.all().filter(input_source='test').order_by('-timestamp')[0]
        time_from_test = last_test_measurement.timestamp + 1
    except:
        time_from_test = 0

    time_to = int(
        (
            datetime.datetime.today() + 
            datetime.timedelta(days=30) - 
            datetime.datetime(1970, 1, 1)
        ).total_seconds()
    )
    
    measurements = google_fit.get_heart_rate_data_from_cinch(time_from_cinch, time_to)
    measurements = measurements + google_fit.get_heart_rate_data_from_test(time_from_test, time_to)
    
    for m in measurements:
        heart_rate_measurement = HeartRateMeasurement(
            user_id = 11262861,
            input_source=m['source'],
            measurement_unit='bpm',
            timestamp=m['timestamp'],
            timezone='Europe/Bucharest',
            value=m['value']
        )

        logger.debug("[medical-compliance] Saving heart rate measurement: %s" % (heart_rate_measurement))
        heart_rate_measurement.save()

        logger.debug("[medical-compliance] Broadcasting heart rate measurement: %s" % (heart_rate_measurement))
        broadcast_measurement('heartrate', heart_rate_measurement)
    
    logger.debug("[medical-compliance] Sending the last cinch heart rate measurement for analysis: %s" % (last_cinch_measurement))
    analyze_heart_rates.delay(last_cinch_measurement, 'cinch')

    logger.debug("[medical-compliance] Sending the last test heart rate measurement for analysis: %s" % (last_test_measurement))
    analyze_heart_rates.delay(last_test_measurement, 'test')

    return json.dumps(measurements, indent=4, sort_keys=True)

@app.task(name='medical_compliance_measurements.process_steps_measurement')
def process_steps_measurement():
    logger.debug("[medical-compliance] Process steps measurement: %s. Retrieving test and google fit steps measurements since the last one..." % (locals()))

    last_google_fit_measurement = None
    last_test_measurement = None
    
    google_fit_steps_measurements = StepsMeasurement.objects.all().filter(input_source='google_fit').order_by('-start_timestamp')
    logger.debug("[medical-compliance] All google_fit_steps_measurements: " % (google_fit_steps_measurements))

    test_steps_measurements = StepsMeasurement.objects.all().filter(input_source='test').order_by('-start_timestamp')
    logger.debug("[medical-compliance] All test_steps_measurements: " % (test_steps_measurements))

    try:
        last_google_fit_measurement = StepsMeasurement.objects.all().filter(input_source='google_fit').order_by('-start_timestamp')[0]
        time_from_google_fit = last_google_fit_measurement.start_timestamp + 1
    except Exception as e:
        logger.debug("[medical-compliance] Error retrieving last google fit steps measurement: %s" % (e))
        time_from_google_fit = 0

    try:
        last_test_measurement = StepsMeasurement.objects.all().filter(input_source='test').order_by('-start_timestamp')[0]
        time_from_test = last_test_measurement.start_timestamp + 1
    except Exception as e:
        logger.debug("[medical-compliance] Error retrieving last test steps measurement: %s" % (e))
        time_from_test = 0

    time_to = int(
        (
            datetime.datetime.today() + 
            datetime.timedelta(days=30) - 
            datetime.datetime(1970, 1, 1)
        ).total_seconds()
    )

    measurements = []
    try:
        measurements = google_fit.get_steps_data_from_google_fit(time_from_google_fit, time_to)
    except Exception as e:
        logger.debug("[medical-compliance] Error retrieving steps from google fit: %s." % (e))
    
    test_measurements = []
    try:
        test_measurements = google_fit.get_steps_data_from_test(time_from_test, time_to)
    except Exception as e:
        logger.debug("[medical-compliance] Error retrieving steps from test data stream: %s." % (e))

    measurements = measurements + test_measurements
    
    logger.debug("[medical-compliance] Merged step measurements: %s." % (measurements))

    for m in measurements:
        steps_measurement = StepsMeasurement(
            user_id = 11262861,
            input_source=m['source'],
            measurement_unit='steps',
            start_timestamp=m['start_timestamp'],
            end_timestamp=m['end_timestamp'],
            timezone='Europe/Bucharest',
            value=m['value']
        )

        logger.debug("[medical-compliance] Saving steps measurement: %s" % (steps_measurement))
        steps_measurement.save()

    return json.dumps(measurements, indent=4, sort_keys=True)

def broadcast_measurement(measurement_type, measurement):
    measurement_json = {
        'type': measurement_type,
        'user_id': measurement.user_id,
        'input_source': measurement.input_source,
        'measurement_unit': measurement.measurement_unit,
        'timestamp': measurement.timestamp,
        'timezone': measurement.timezone,
        'value': measurement.value
    }
    
    global_app.send_task('cami.on_measurement_received', [measurement_json], queue='broadcast_measurement')
    
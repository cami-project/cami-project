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

from models import WithingsMeasurement
from withings import WithingsCredentials, WithingsApi, WithingsMeasures

from tasks import fetch_weight_measurement

logger = get_task_logger("withings_controller.save_measurement")


app = Celery('api.tasks', broker=settings.BROKER_URL)
app.conf.update(
    CELERY_DEFAULT_QUEUE='withings_measurements',
    CELERY_QUEUES=(
        Queue('withings_measurements', Exchange('withings_measurements'), routing_key='withings_measurements'),
    ),
)


@app.task(name='withings_controller.save_measurement')
def save_measurement(userid, start_ts, end_ts, measurement_type_id):
    logger.debug(
        "Sending Withings request for measurement retrieval for { userid: %s, start ts: %s, end ts: %s, type: %s }" %
        (userid, start_ts, end_ts, measurement_type_id)
    )

    credentials = WithingsCredentials(access_token=settings.WITHINGS_OAUTH_V1_TOKEN,
                                      access_token_secret=settings.WITHINGS_OAUTH_V1_TOKEN_SECRET,
                                      consumer_key=settings.WITHINGS_CONSUMER_KEY,
                                      consumer_secret=settings.WITHINGS_CONSUMER_SECRET,
                                      user_id=userid)
    # TODO: modify storage of user credentials in settings file to a per userid basis

    client = WithingsApi(credentials)
    req_params = {
        'startdate': start_ts,
        'enddate': end_ts,
        'meastype': int(measurement_type_id)
    }
    response = client.request('measure', 'getmeas', req_params)

    logger.debug(
        "Got the following Withings response for user_id %s and req params %s: %s" %
        (userid, req_params, response)
    )

    measures = WithingsMeasures(response)
    timezoneStr = response['timezone']
    measurement_type = WithingsMeasurement.get_measure_type_by_id(int(measurement_type_id))

    for m in measures:
        meas = WithingsMeasurement(
            withings_user_id = int(userid),
            type=measurement_type_id,
            retrieval_type=m.attrib,
            measurement_unit=WithingsMeasurement.MEASUREMENT_SI_UNIT[measurement_type],
            timestamp=m.data['date'],
            timezone=timezoneStr,
            value=m.__getattribute__(measurement_type))
        meas.save()

        fetch_weight_measurement.delay(userid, "withings", meas.measurement_unit, meas.timestamp, meas.timezone, meas.value)
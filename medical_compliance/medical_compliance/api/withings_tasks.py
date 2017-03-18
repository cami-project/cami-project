"""
Define tasks that can be sent by other services through the RabbitMQ broker.
All tasks have manually defined names instead of automatic[1] to eliminate the
need to have the same module structure in the worker and the client.
[1] http://docs.celeryproject.org/en/latest/userguide/tasks.html#task-naming-relative-imports
"""

from celery import Celery
from celery.utils.log import get_task_logger
from kombu import Queue, Exchange
from django.conf import settings #noqa

from models import WithingsMeasurement
from withings import WithingsCredentials, WithingsApi, WithingsMeasures

from tasks import process_weight_measurement
import store_utils

logger = get_task_logger("withings_controller.retrieve_and_save_withings_measurements")


app = Celery('api.tasks', broker=settings.BROKER_URL)
app.conf.update(
    CELERY_DEFAULT_QUEUE='withings_measurements',
    CELERY_QUEUES=(
        Queue('withings_measurements', Exchange('withings_measurements'), routing_key='withings_measurements'),
    ),
)


@app.task(name='withings_controller.retrieve_and_save_withings_measurements')
def retrieve_and_save_withings_measurements(withings_userid, device_id, start_ts, end_ts, measurement_type_id):
    logger.debug("[medical-compliance] Query Withings API to retrieve measurement: %s" % (locals()))

    ## retrieve the DeviceUsage instance associated with the Withings `withings_userid` and `device_id`
    device_usage_data = store_utils.get_device_usage(store_utils.STORE_ENDPOINT_URI, device = device_id,
                                                     access_info = {"withings_userid" : str(withings_userid)})



    if not device_usage_data:
        logger.error("[medical-compliance] Cannot find any user - device combination with access config for "
                     "withings_userid : %s and device_id : %s" % (str(withings_userid), str(device_id)))
        return

    logger.debug("[medical-compliance] DeviceUsage data for Withings weight measurement: %s" % str(device_usage_data))

    ## get access_info, user and device information from device_usage object
    access_info = device_usage_data['access_info']
    cami_user_id = device_usage_data['user_id']

    credentials = WithingsCredentials(access_token=access_info['withings_oauth_token'],
                                      access_token_secret=access_info['withings_oauth_token_secret'],
                                      consumer_key=access_info['withings_consumer_key'],
                                      consumer_secret=access_info['withings_consumer_secret'],
                                      user_id=int(access_info['withings_userid']))

    client = WithingsApi(credentials)
    req_params = {
        'startdate': start_ts,
        'enddate': end_ts,
        'meastype': int(measurement_type_id)
    }
    response = client.request('measure', 'getmeas', req_params)

    logger.debug(
        "[medical-compliance] Got the following Withings response for user_id %s and req params %s: %s" %
        (withings_userid, req_params, response)
    )

    measures = WithingsMeasures(response)
    timezoneStr = response['timezone']
    measurement_type = WithingsMeasurement.get_measure_type_by_id(int(measurement_type_id))

    for m in measures:
        ## Store WithingsMeasurement for error inspection
        meas = WithingsMeasurement(
            withings_user_id = int(withings_userid),
            type=measurement_type_id,
            retrieval_type=m.attrib,
            measurement_unit=WithingsMeasurement.MEASUREMENT_SI_UNIT[measurement_type],
            timestamp=m.data['date'],
            timezone=timezoneStr,
            value=m.__getattribute__(measurement_type))

        logger.debug("[medical-compliance] Saving Withings measurement in cami DB: %s" % (meas))
        meas.save()

        logger.debug("[medical-compliance] Sending Withings weight measurement for processing: %s" % (meas))
        process_weight_measurement.delay(cami_user_id,              # end user id
                                         device_id,                 # device id
                                         "weight",                  # measurement_type
                                         meas.measurement_unit,
                                         meas.timestamp,
                                         meas.timezone,
                                         meas.value)
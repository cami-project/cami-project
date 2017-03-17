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
from django.core.exceptions import ObjectDoesNotExist
from notifications_adapter import NotificationsAdapter

from .. import store_utils

DELTA_WEIGHT = 2

logger = get_task_logger("medical_compliance_weight_analyzers.analyze_weight")

app = Celery('api.tasks', broker=settings.BROKER_URL)
app.conf.update(
    CELERY_DEFAULT_QUEUE='medical_compliance_weight_analyzers',
    CELERY_QUEUES=(
        Queue('medical_compliance_weight_analyzers', Exchange('medical_compliance_weight_analyzers'), routing_key='medical_compliance_weight_analyzers'),
    ),
)

@app.task(name='medical_compliance_weight_analyzers.analyze_weights')
def analyze_weights(weight_measurement_id, user_id, device_id):
    analyze_last_two_weights(weight_measurement_id, user_id, device_id)


def get_previous_weight_measures(reference_id, user_id, weights_count):
    ## first get measurement by reference_id
    endpoint_host_uri = "http://" + store_utils.STORE_HOST + ":" + store_utils.STORE_PORT
    retrieved_measurement= store_utils.get_measurements(endpoint_host_uri, id=reference_id, limit=1)

    if retrieved_measurement:
        ## retrieve previous `weights_count` measurements, if they exist
        last_weight_measurements = store_utils.get_measurements(endpoint_host_uri,
                                                                timestamp__lte = retrieved_measurement['timestamp'],
                                                                measurement_type = retrieved_measurement['measurement_type'],
                                                                user = user_id,
                                                                order_by = "-timestamp",
                                                                limit = weights_count)

        if not last_weight_measurements:
            return []

        return last_weight_measurements
    else:
        logger.debug("[medical-compliance] No measurement with id=%s found in CAMI Store." % reference_id)
        return []


# TODO: this is a dummy module and should be generalized at least with a task structure
# all the tasks should listen on the same weight queue and all of them should compute some metrics (broadcast?)
def analyze_last_two_weights(weight_measurement_id, user_id, device_id):
    logger.debug("[medical-compliance] Analyze weights request: %s. Trying to retrieve the last two weights..." % (locals()))

    measurement_list = get_previous_weight_measures(weight_measurement_id, user_id, device_id, 2)
    logger.debug("[medical-compliance] The last two weights: %s" % str(measurement_list))


    if len(measurement_list) == 2:
        current_measurement= measurement_list[0]
        previous_measurement = measurement_list[1]
        
        logger.debug("[medical-compliance] Checking if the difference between the last two weight measurements is > %s" % (DELTA_WEIGHT))

        delta_value = current_measurement['value_info']['value'] - previous_measurement['value_info']['value']
        notifications_adapter = NotificationsAdapter()

        if delta_value <= -1 * DELTA_WEIGHT:
            logger.debug("[medical-compliance] New weight measurement < last one. Difference: %s. Sending notifications..." % (delta_value))

            message = u"Jim lost %s kg" % (abs(delta_value))
            description = "You can contact him and see what's wrong."
            # notifications_adapter.send_caregiver_notification(withings_user_id, "weight", "medium", message, description)
            notifications_adapter.send_caregiver_notification(user_id, "weight", "medium", message, description)

            message = u"There's a decrease of %s kg in your weight." % (abs(delta_value))
            description = "Please take your meals regularly."
            # notifications_adapter.send_elderly_notification(withings_user_id, "weight", "medium", message, description)
            notifications_adapter.send_elderly_notification(user_id, "weight", "medium", message, description)

        elif delta_value >= DELTA_WEIGHT:
            logger.debug("[medical-compliance] New weight measurement > last one. Difference: %s. Sending notifications..." % (delta_value))

            message = u"Jim gained %s kg" % (abs(delta_value))
            description = "Please check if this has to do with his diet."
            # notifications_adapter.send_caregiver_notification(withings_user_id, "weight", "medium", message, description)
            notifications_adapter.send_caregiver_notification(user_id, "weight", "medium", message, description)

            message = u"There's an increase of %s kg in your weight." % (abs(delta_value))
            description = "Please be careful with your meals."
            # notifications_adapter.send_elderly_notification(withings_user_id, "weight", "medium", message, description)
            notifications_adapter.send_elderly_notification(user_id, "weight", "medium", message, description)
        
        else:
            logger.debug("[medical-compliance] Weight difference < delta_weight (%s kg). Not sending notifications." % (delta_value))

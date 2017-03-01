from __future__ import absolute_import

import os
from celery import Celery
from celery.utils.log import get_task_logger

# set the default Django settings module for the 'celery' program.
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'store.settings')
from django.conf import settings #noqa

logger = get_task_logger('store.activity_sync')

app = Celery('store', broker=settings.BROKER_URL)
app.config_from_object('django.conf:settings')
app.autodiscover_tasks(lambda: settings.INSTALLED_APPS)

from kombu import Queue, Exchange
from .gcal_activity_backend import *
from .models import UserAccount, Activity
from .constants import ACTIVITY_TYPE, ACTIVITY_SOURCE, ACTIVITY_LOCAL_ID, CALENDAR_BACKEND_SERVICE
import dateutil.parser
import pytz

@app.task(name='store_activity_sync_local.sync_locally')
def sync_locally(user_account_uuid, activities, insert = True, sync_after_local_create = False):
    """
    The task performs a local sync of activities in the following cases:
      - retrieve them by periodic weekly task, or create them from the UI - insert = True
      - perform a local update, synchronize remotely and then confirm synchronization,
        i.e. update the is_synced flag - insert = False
      - create a local activity, synchronize remotely and the update
        the `backend_id` and `is_synced` flags locally - insert = False, sync_after_local_create = True

    :param user_account_uuid:
    :param activities:
    :param insert:
    :param sync_after_local_create:
    :return:
    """
    logger.debug("[store_activity_sync] Synchronizing activity data from GCal backend to local database. Performing INSERT = %s: %s" % (locals(), str(insert)))

    if insert:
        # if we have an INSERT we create the activity objects and perform a bulk update
        created_activities = []

        # get the UserAccount object for the specified id
        user_account = UserAccount.objects.get(uuid=user_account_uuid)

        for activity in activities:
            # setup start and end times of the activity
            timeZone = activity['start']['timeZone']
            start_str = activity['start']['dateTime']
            end_str = activity['end']['dateTime']

            start = dateutil.parser.parse(start_str)
            end = dateutil.parser.parse(end_str)

            start = start.replace(tzinfo = pytz.timezone(timeZone))
            end = end.replace(tzinfo = pytz.timezone(timeZone))

            # set recursive flag
            recursive = False
            if activity.get('recurrence', None):
                recursive = True

            activity_obj = Activity(
                name = activity['summary'],
                backend_id = activity['id'],
                user = user_account,
                activity_type = activity['extendedProperties']['private'][ACTIVITY_TYPE],
                activity_source = activity['extendedProperties']['private'][ACTIVITY_SOURCE],

                starts_at = start,
                ends_at = end,
                is_recursive = recursive,
                is_synced = True
            )

            created_activities.append(activity_obj)

        Activity.objects.bulk_create(created_activities)
        logger.debug("[store_activity_sync] %s activities synchronized locally." % (str(len(activities))))

    else:
        if not sync_after_local_create:
            # Otherwise it means we are handling a confirmation that the remote backend update has succeeded.
            # Therefore, we perform an IN filter for the set of updated activity IDs and set the `is_synced` flag to TRUE.
            activity_ids = [activity['id'] for activity in activities]
            Activity.objects.filter(backend_id__in = [activity_ids]).update(is_synced=True)
            logger.debug("[store_activity_sync] Confirmed sync of %s locally modified activities." % (str(len(activities))))
        else:
            # In this case we are handling a confirmation of a successful remote sync where the backend activity
            # has been created and we must insert back the obtained backend_id, as well as update the is_synced flag
            # We have to do this on a per individual activity basis
            for activity in activities:
                if ACTIVITY_LOCAL_ID in activity['extendedProperties']['private']:
                    Activity.objects.filter(id = activity['extendedProperties']['private'][ACTIVITY_LOCAL_ID]).\
                        update(backend_id = activity['id'], is_synced = True)

                    logger.debug("[store_activity_sync] Confirmed sync of locally created activity with name %s and backend_id %s" % (activity['summary'], activity['id']))
                else:
                    logger.error("[store_activity_sync] Error finding local activity for synchronization confirmation. " +
                                 "No `activity_local_id` found in private properties of remote activity with backend_id %s" % activity['id'])


    # logger.debug("[medical-compliance] Saving weight measurement: %s" % (weight_measurement))
    # weight_measurement.save()
    #
    # logger.debug("[medical-compliance] Sending the weight measurement with id %s for analysis." % (weight_measurement.id))
    # analyze_weights.delay(weight_measurement.id)
    #
    # logger.debug("[medical-compliance] Broadcasting weight measurement: %s" % (weight_measurement))
    # broadcast_measurement('weight', weight_measurement)


@app.task(name='store_activity_sync_remote.sync_remote')
def sync_remote(user_account_uuid, local_activity, insert = True):
    logger.debug("[store_activity_sync] Synchronizing activity data from local store to GCal backend. " +
                 "Performing INSERT = %s: %s" % (locals(), str(insert)))

    ## first retrieve the `calendar_id` required to synchronize to the correct user schedule
    user = UserAccount.objects.get(uuid = user_account_uuid)
    user_calendar = user.used_interface_services.get(name=CALENDAR_BACKEND_SERVICE)

    ## TODO: here we must ensure we actually have such an GCal configuration object
    calendar_id = user_calendar.connection_info['calendar_id']

    credentials = get_credentials()
    calendar_service = get_calendar_service(credentials)

    if insert:
        # if we have an INSERT we call the insert helper function
        # this returns the created event dict which we use to enqueue a local sync task which only updates
        # the `is_synced` flag
        start = local_activity.starts_at.replace(tzinfo=None)
        end = local_activity.ends_at.replace(tzinfo=None)

        backed_activity, status = create_single_activity(calendar_service, calendar_id, local_activity.name,
                                                         start_date = start, end_date = end,
                                                         timezone_spec = "Europe/Bucharest",
                                                         activity_type=local_activity.activity_type,
                                                         activity_source = local_activity.activity_source,
                                                         activity_local_id = local_activity.id)

        if status != 200:
            logger.error("[store_activity_sync] Locally created activity %s with ID %s was not synchronized remotely" % local_activity.name, local_activity.id)
        else:
            # start task to confirm that the locally created activity has been backed up remotely.
            # Update the backend_id and is_synced fields
            logger.debug("[store_activity_sync] Start local sync task for confirming that activity with local ID %s has \
                         been remotely backed up with backend_id %s" % (local_activity.id, backed_activity['id']))
            sync_locally.delay(user_account_uuid, [backed_activity], insert = False, sync_after_local_create = True)

    else:
        # Otherwise it means we are handling an update request for a locally modified activity which needs to be
        # synchronized remotely
        start = local_activity.starts_at.replace(tzinfo=None)
        end = local_activity.ends_at.replace(tzinfo=None)

        current_backed_activity, status = get_activity(calendar_service, calendar_id, local_activity.backend_id)
        if status != 200:
            logger.error("[store_activity_sync] Locally modified activity %s with ID %s cannot be synced remotely. " +
                         "Cannot retrieve corresponding backed activity with backend_id %s" \
                         % (local_activity.name, local_activity.id, local_activity.backend_id))
        else:
            backed_activity, status = postpone_activity(calendar_service, calendar_id,
                                                        activity_data = current_backed_activity,
                                                        new_start_date = start,
                                                        new_end_date = end,
                                                        timezone_spec = "Europe/Bucharest")

            if status != 200:
                logger.error(
                    "[store_activity_sync] Locally modified activity %s with backend_id %s cannot be synced remotely. " +
                    "Reason: %s" % (local_activity.name, local_activity.backend_id, backed_activity))
            else:
                logger.debug("[store_activity_sync] Confirmed remote sync of locally modified activity %s with backend_id %s."
                             % (local_activity.name, local_activity.backend_id))
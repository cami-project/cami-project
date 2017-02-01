import sys
import os

from celery.utils.log import get_task_logger
from django.conf import settings

# Add google_fit folder to python path
# TODO: find a cleaner way to do this
sys.path.append(os.path.join(os.path.dirname(__file__), '..', '..', '..', 'google_fit'))
from client.GoogleFitClient import GoogleFitClient

logger = get_task_logger('medical_compliance.google_fit')

CINCH_OFFSET_SECONDS = 10800

def test_transform(measurement):
    measurement['source'] = 'test'
    return measurement

def cinch_transform(heart_rate):
    heart_rate['timestamp'] = heart_rate['timestamp'] + CINCH_OFFSET_SECONDS
    heart_rate['source'] = 'cinch'
    return heart_rate

def google_fit_transform(measurement):
    measurement['source'] = 'google_fit'
    return measurement

def get_google_fit_client():
    return GoogleFitClient(
        settings.GOOGLE_FIT_CLIENT_ID,
        settings.GOOGLE_FIT_CLIENT_SECRET,
        settings.GOOGLE_FIT_REFRESH_TOKEN
    )

def get_heart_rate_data_from_cinch(time_from, time_to):
    logger.debug("[medical-compliance] Google Fit - get HR data from cinch: %s" % (locals()))
    
    # Create GoogleFitClient object
    google_fit = get_google_fit_client()

    # Get data from Cinch
    heart_rate_data = google_fit.get_hr_data(
        settings.GOOGLE_FIT_HR_DATASTREAM_ID, 
        time_from - CINCH_OFFSET_SECONDS if time_from >= CINCH_OFFSET_SECONDS else 0,
        time_to - CINCH_OFFSET_SECONDS,
        cinch_transform
    )

    logger.debug("[medical-compliance] Google Fit - HR data from cinch: %s" % (heart_rate_data))

    return heart_rate_data

def get_heart_rate_data_from_test(time_from, time_to):
    logger.debug("[medical-compliance] Google Fit - get HR data from test: %s" % (locals()))

    # Create GoogleFitClient object
    google_fit = get_google_fit_client()

    heart_rate_data = []

    # Get data from CAMI Test
    datastream_id = google_fit.get_datastream_id_by_name(settings.GOOGLE_FIT_HR_TEST_DATASTREAM_NAME)
    if datastream_id:
        heart_rate_data = google_fit.get_hr_data(
            datastream_id, 
            time_from,
            time_to,
            test_transform
        )
    
    logger.debug("[medical-compliance] Google Fit - HR data from test (datastream_id = %s): %s" % (datastream_id, heart_rate_data))

    return heart_rate_data

def get_steps_data_from_google_fit(time_from, time_to):
    logger.debug("[medical-compliance] Google Fit - get steps data from google fit: %s" % (locals()))
    
    # Create GoogleFitClient object
    google_fit = get_google_fit_client()

    steps_data = []

    # Get data from Google Fit
    datastream_id = get_lg_watch_steps_datastream_id()
    if datastream_id:        
        steps_data = google_fit.get_step_data(
            datastream_id, 
            time_from,
            time_to,
            google_fit_transform
        )
    else:
        logger.debug("[medical-compliance] Could not retrive google_fit steps datasource")

    logger.debug("[medical-compliance] Google Fit - steps data from google_fit: %s" % (steps_data))

    return steps_data

def get_steps_data_from_test(time_from, time_to):
    logger.debug("[medical-compliance] Google Fit - get steps data from test: %s" % (locals()))

    # Create GoogleFitClient object
    google_fit = get_google_fit_client()

    steps_data = []

    # Get data from CAMI Test
    datastream_id = google_fit.get_datastream_id_by_name(settings.GOOGLE_FIT_STEPS_TEST_DATASTREAM_NAME)
    if datastream_id:
        steps_data = google_fit.get_step_data(
            datastream_id, 
            time_from,
            time_to,
            test_transform
        )
    else:
        logger.debug("[medical-compliance] Could not retrive test steps datasource")

    logger.debug("[medical-compliance] Google Fit - Steps data from test (datastream_id = %s): %s" % (datastream_id, steps_data))

    return steps_data

def get_lg_watch_steps_datastream_id():
    google_fit = get_google_fit_client()
    all_data_sources = google_fit.get_all_datastreams()
    
    for ds in all_data_sources['dataSource']:
        if 'dataStreamName' in ds and ds['dataStreamName'].startswith('derive_step_deltas<-raw:com.google.step_count.cumulative:LGE:LG Watch Urbane:') and ds['dataStreamName'].endswith(':Step Counter'):
            return ds['dataStreamId']
    
    return None
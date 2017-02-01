import sys
import os
sys.path.append(os.path.join(os.path.dirname(__file__), '..'))

import time
from datetime import datetime
import json

import client.config as config
from client.GoogleFitClient import GoogleFitClient

# Open the refresh token file
token_file = open("refresh_token", "r")

# Create GoogleFitHeartRate object
google_fit_client = GoogleFitClient(config.CLIENT_ID, config.CLIENT_SECRET, token_file.read())

date_from = "15/01/2017"
date_to = "30/12/2017"

# Convert dates to timestamps
date_from_converted = int(
    time.mktime(datetime.strptime(date_from, "%d/%m/%Y").timetuple())
)
date_to_converted = int(
    time.mktime(datetime.strptime(date_to, "%d/%m/%Y").timetuple())
)

# Get data from Google Fit step counter only
google_fit_step_counter_ds_id = config.GOOGLE_FIT_STEPS_DATASTREAM_ID
print("Getting Google Fit step data from: %s" % (google_fit_step_counter_ds_id))
step_data = google_fit_client.get_step_data(
    google_fit_step_counter_ds_id, 
    date_from_converted,
    date_to_converted
)
print json.dumps(step_data, indent=4, sort_keys=True)

# Get data from Google Fit test step counter only
cami_steps_ds_id = google_fit_client.get_datastream_id_by_name(config.GOOGLE_FIT_STEPS_TEST_DATASTREAM_NAME)
print ("Data from %s stream (this a test stream - %s): " % (cami_steps_ds_id, config.GOOGLE_FIT_STEPS_TEST_DATASTREAM_NAME))
step_data = google_fit_client.get_step_data(
    cami_steps_ds_id, 
    date_from_converted,
    date_to_converted
)
print json.dumps(step_data, indent=4, sort_keys=True)





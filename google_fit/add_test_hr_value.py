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

value = sys.argv[1]

hr_test_ds_name = config.GOOGLE_FIT_HR_TEST_DATASTREAM_NAME
hr_test_ds_id = google_fit_client.get_datastream_id_by_name(hr_test_ds_name)
if hr_test_ds_id == None:
    try:
        hr_test_ds_id = google_fit_client.create_hr_datastream(hr_test_ds_name)
    except Exception as e:
        print("Error creating Google Fit data stream: %s" % (e))
        raise

result = google_fit_client.write_hr_value(value, hr_test_ds_id)
print(result)
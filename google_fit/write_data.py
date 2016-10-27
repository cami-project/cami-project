import sys
import time
import json

from datetime import datetime

import config
from GoogleFitHeartRate import GoogleFitHeartRate

# Set value to be written
value = sys.argv[1]

# Open the refresh token file
token_file = open("refresh_token", "r")

# Create GoogleFitHeartRate object
google_fit = GoogleFitHeartRate(config.CLIENT_ID, config.CLIENT_SECRET, token_file.read())

# Close the refresh token file
token_file.close()

# Get datastream id of CAMI Test
datastream_id = google_fit.get_datastream_id_by_name("CAMI Heart Rate Test")

# If the datastream does not exist, create it
if datastream_id is None:
    datastream_id = google_fit.create_datastream("CAMI Heart Rate Test")

# Write data to CAMI Test datastream
result = google_fit.write_value(value, datastream_id)

# Print the result
print json.dumps(result, indent=4, sort_keys=True)
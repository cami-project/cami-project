import time
import json

from datetime import datetime

import config
from GoogleFitHeartRate import GoogleFitHeartRate

# Open the refresh token file
token_file = open("refresh_token", "r")

# Create GoogleFitHeartRate object
google_fit = GoogleFitHeartRate(config.CLIENT_ID, config.CLIENT_SECRET, token_file.read())

# Close the refresh token file
token_file.close()

date_from = "01/01/2016"
date_to = "30/12/2016"

# Convert dates to timestamps
date_from_converted = int(
    time.mktime(datetime.strptime(date_from, "%d/%m/%Y").timetuple())
)
date_to_converted = int(
    time.mktime(datetime.strptime(date_to, "%d/%m/%Y").timetuple())
)

# Get data from Cinch only
heart_rate_data = google_fit.get_data(
    "raw:com.google.heart_rate.bpm:com.ryansteckler.perfectcinch:", 
    date_from_converted,
    date_to_converted
)

# Print data
print json.dumps(heart_rate_data, indent=4, sort_keys=True)
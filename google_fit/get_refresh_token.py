import time
import json

from datetime import datetime

import config
from GoogleFitHeartRate import GoogleFitHeartRate

# Open the refresh token file
token_file = open("refresh_token", "w")

# Get the refresh token
refresh_token = GoogleFitHeartRate.get_refresh_token(config.CLIENT_ID, config.CLIENT_SECRET)

# Write the token to the file
token_file.write(refresh_token)

# Close the refresh token file
token_file.close()

# Print an informative message
print "The token was written in the file! Now you can call the other scripts"
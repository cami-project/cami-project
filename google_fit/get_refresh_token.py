import sys
import os
sys.path.append(os.path.join(os.path.dirname(__file__), '..'))

from client.GoogleFitClient import GoogleFitClient

import time
import json

from datetime import datetime

import client.config as config

# Open the refresh token file
token_file = open("refresh_token", "w")

# Get the refresh token
refresh_token = GoogleFitClient.get_refresh_token(config.CLIENT_ID, config.CLIENT_SECRET)

# Write the token to the file
token_file.write(refresh_token)

# Close the refresh token file
token_file.close()

# Print an informative message
print "The token was written in the file! Now you can call the other scripts"
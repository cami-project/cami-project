import time
import json
import requests

from datetime import datetime
from requests_oauthlib import OAuth2Session

# Open the refresh token file
token_file = open("refresh_token", "r")

# Credentials
client_id = '701996606933-17j7km8f8ce8vohhdcnur453cbn44aau.apps.googleusercontent.com'
client_secret = 'K-lZ7t49-Gvhtz2P-RTqBhAQ'
token_url = "https://accounts.google.com/o/oauth2/token"

token = {
    'access_token': 'empty',
    'refresh_token': token_file.read(),
    'token_type': 'Bearer',
    'expires_in': '-30'
}

extra = {
    'client_id': client_id,
    'client_secret': client_secret
}

def token_saver(token):
    pass

# Authentication
client = OAuth2Session(
	client_id,
	token=token,
	auto_refresh_url=token_url,
    auto_refresh_kwargs=extra,
 	token_updater=token_saver
)

date_from = "01/01/2016"
date_to = "30/12/2016"

date_from_converted = str(int(
		time.mktime(datetime.strptime(date_from, "%d/%m/%Y").timetuple())
	)) + "000000000"
date_to_converted = str(int(
		time.mktime(datetime.strptime(date_to, "%d/%m/%Y").timetuple())
	)) + "000000000"
dataset_id = date_from_converted + "-" + date_to_converted

# Get data
r = client.get(
	"https://www.googleapis.com/fitness/v1/users/me/dataSources/"
	"raw:com.google.heart_rate.bpm:com.ryansteckler.perfectcinch:/datasets/" + dataset_id
)

# Parse data
raw_data = json.loads(r.text)
data = []
for p in raw_data['point']:
	hr = {}

	hr['datetime'] = datetime.fromtimestamp(
		int(p['startTimeNanos'][:-9])
	).strftime('%Y-%m-%d %H:%M:%S')
	hr['value'] = int(p['value'][0]['fpVal'])

	data.append(hr)

# Print data
print json.dumps(data, indent=4, sort_keys=True)
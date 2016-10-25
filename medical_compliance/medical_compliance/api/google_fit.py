import time
import json
import requests

from datetime import datetime
from requests_oauthlib import OAuth2Session

from django.conf import settings


TOKEN_URL = "https://accounts.google.com/o/oauth2/token"
REDIRECT_URI = 'http://google.ro'
AUTHORIZATION_BASE_URL = "https://accounts.google.com/o/oauth2/v2/auth"
SCOPE = [
    "https://www.googleapis.com/auth/fitness.body.read"
]

def token_saver(token):
    pass

def get_refresh_token():
	google = OAuth2Session(settings.GOOGLE_FIT_CLIENT_ID, scope=SCOPE, redirect_uri=REDIRECT_URI)

	# Authorize
	authorization_url, state = google.authorization_url(
		AUTHORIZATION_BASE_URL,
	    access_type="offline",
	    prompt="consent",
	)
	print 'Please go here and authorize,', authorization_url

	# Get the authorization verifier code from the callback url
	redirect_response = raw_input('Paste the redirect url here:')

	# Get the tokens
	r = google.fetch_token(TOKEN_URL, client_secret=settings.GOOGLE_FIT_CLIENT_SECRET, authorization_response=redirect_response)

	return r['refresh_token']

def get_heart_rate_data(time_from, time_to):
	token = {
	    'access_token': 'empty',
	    'refresh_token': settings.GOOGLE_FIT_REFRESH_TOKEN,
	    'token_type': 'Bearer',
	    'expires_in': '-30'
	}

	extra = {
	    'client_id': settings.GOOGLE_FIT_CLIENT_ID,
	    'client_secret': settings.GOOGLE_FIT_CLIENT_SECRET
	}

	# Authentication
	client = OAuth2Session(
		settings.GOOGLE_FIT_CLIENT_ID,
		token=token,
		auto_refresh_url=TOKEN_URL,
	    auto_refresh_kwargs=extra,
	 	token_updater=token_saver
	)

	dataset_id = time_from + "-" + time_to

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

		hr['timestamp'] = int(p['startTimeNanos'][:-9])
		hr['value'] = float(p['value'][0]['fpVal'])

		data.append(hr)

	return data
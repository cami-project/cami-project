import json
import requests

from requests_oauthlib import OAuth2Session

import config

# Credentials
token_url = "https://accounts.google.com/o/oauth2/token"
redirect_uri = 'http://google.ro'
authorization_base_url = "https://accounts.google.com/o/oauth2/v2/auth"
scope = [
    "https://www.googleapis.com/auth/fitness.body.read"
]

def get_refresh_token():
	google = OAuth2Session(config.CLIENT_ID, scope=scope, redirect_uri=redirect_uri)

	# Authorize
	authorization_url, state = google.authorization_url(
		authorization_base_url,
	    access_type="offline",
	    prompt="consent",
	)
	print 'Please go here and authorize,', authorization_url

	# Get the authorization verifier code from the callback url
	redirect_response = raw_input('Paste the redirect url here:')

	# Get the tokens
	r = google.fetch_token(token_url, client_secret=config.CLIENT_SECRET, authorization_response=redirect_response)
	return r['refresh_token']

# Open the refresh token file
token_file = open("refresh_token", "w")
token = get_refresh_token()
token_file.write(token)
print "The token was written in the file! Now you can call get_data.py"
import time
import json
import requests

from datetime import datetime
from requests_oauthlib import OAuth2Session

class GoogleFitHeartRate:
    TOKEN_URL = "https://accounts.google.com/o/oauth2/token"
    REDIRECT_URI = 'http://google.ro'
    AUTHORIZATION_BASE_URL = "https://accounts.google.com/o/oauth2/v2/auth"
    SCOPE = [
        "https://www.googleapis.com/auth/fitness.body.read",
        "https://www.googleapis.com/auth/fitness.body.write"
    ]

    def __init__(self, client_id, client_secret, refresh_token):
        token = {
            'access_token': 'empty',
            'refresh_token': refresh_token,
            'token_type': 'Bearer',
            'expires_in': '-30'
        }

        extra = {
            'client_id': client_id,
            'client_secret': client_secret
        }

        # Authentication
        self.client = OAuth2Session(
            client_id,
            token=token,
            auto_refresh_url=GoogleFitHeartRate.TOKEN_URL,
            auto_refresh_kwargs=extra,
            token_updater=self.token_saver
        )

    def token_saver(self, token):
        pass

    def get_datastream_id_by_name(self, name):
        r = self.client.get("https://www.googleapis.com/fitness/v1/users/me/dataSources/")
        data_sources = json.loads(r.text)

        for ds in data_sources['dataSource']:
            if 'name' in ds and name in ds['name']:
                return ds['dataStreamId']

        return None

    def create_datastream(self, name):
        headers = {'Accept' : 'application/json', 'Content-Type' : 'application/json'}
        data = {
            "name": name, 
            "dataStreamName": "", 
            "dataType": {
                "field": [
                    {
                        "name": "bpm", 
                        "format": "floatPoint"
                    }
                ], 
                "name": "com.google.heart_rate.bpm"
            }, 
            "application": {
                "name": name
            },
            "type": "raw"
        }

        r = self.client.post(
            "https://www.googleapis.com/fitness/v1/users/me/dataSources",
            data=json.dumps(data),
            headers=headers
        )

        datastream = json.loads(r.text)

        return datastream['dataStreamId']

    def write_value(self, value, datastream_id):
        timestamp = str(int(
            (datetime.now() - datetime(1970,1,1)).total_seconds()
        )) + '000000000'
        dataset_id = timestamp + "-" + timestamp

        headers = {'Accept' : 'application/json', 'Content-Type' : 'application/json'}
        data = {
            "minStartTimeNs": timestamp,
            "maxEndTimeNs": timestamp,
            "dataSourceId": datastream_id,
            "point": [
                {
                    "startTimeNanos": timestamp,
                    "endTimeNanos": timestamp,
                    "dataTypeName": "com.google.heart_rate.bpm",
                    "originDataSourceId": datastream_id,
                    "value": [
                        {
                            "fpVal": value,
                            "mapVal": []
                        }
                    ],
                    "modifiedTimeMillis": timestamp[:-9]
                }
            ]
        }

        r = self.client.patch(
            "https://www.googleapis.com/fitness/v1/users/me/dataSources/"
             + datastream_id + "/datasets/" + dataset_id,
            data=json.dumps(data),
            headers=headers
        )

        return json.loads(r.text)

    def get_data(self, datastream_id, time_from, time_to):
        dataset_id = time_from + "-" + time_to

        r = self.client.get(
            "https://www.googleapis.com/fitness/v1/users/me/dataSources/"
            + datastream_id + "/datasets/" + dataset_id
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

    @staticmethod
    def get_refresh_token(client_id, client_secret):
        google = OAuth2Session(
            client_id,
            scope=GoogleFitHeartRate.SCOPE,
            redirect_uri=GoogleFitHeartRate.REDIRECT_URI
        )

        # Authorize
        authorization_url, state = google.authorization_url(
            GoogleFitHeartRate.AUTHORIZATION_BASE_URL,
            access_type="offline",
            prompt="consent",
        )
        print 'Please go here and authorize,', authorization_url

        # Get the authorization verifier code from the callback url
        redirect_response = raw_input('Paste the redirect url here:')

        # Get the tokens
        r = google.fetch_token(
            GoogleFitHeartRate.TOKEN_URL,
            client_secret=client_secret,
            authorization_response=redirect_response
        )

        return r['refresh_token']
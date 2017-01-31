import time
import json
import requests

from datetime import datetime
from requests_oauthlib import OAuth2Session

class GoogleFitClient:
    TOKEN_URL = "https://accounts.google.com/o/oauth2/token"
    REDIRECT_URI = 'http://google.ro'
    AUTHORIZATION_BASE_URL = "https://accounts.google.com/o/oauth2/v2/auth"
    SCOPE = [
        "https://www.googleapis.com/auth/fitness.activity.read",
        "https://www.googleapis.com/auth/fitness.activity.write",
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
            auto_refresh_url=GoogleFitClient.TOKEN_URL,
            auto_refresh_kwargs=extra,
            token_updater=self.token_saver
        )

    def token_saver(self, token):
        pass
    
    def get_all_datastreams(self):
        r = self.client.get("https://www.googleapis.com/fitness/v1/users/me/dataSources/")
        return json.loads(r.text)
    
    def get_datastream_by_name(self, name):
        all_data_sources = self.get_all_datastreams()
        
        for ds in all_data_sources['dataSource']:
            if 'name' in ds and name in ds['name']:
                return ds

        return None
    
    def get_datastream_id_by_name(self, name):
        found_data_source = self.get_datastream_by_name(name)

        if found_data_source == None:
            return None

        return found_data_source['dataStreamId']
    
    def create_hr_datastream(self, name):
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

        response = json.loads(r.text)

        if response.get("error"):
            error_message = response["error"]["message"]
            raise Exception(error_message)
        else:     
            return response["dataStreamId"]
    
    def create_steps_datastream(self, name):
        headers = {'Accept' : 'application/json', 'Content-Type' : 'application/json'}
        data = {
            "name": name, 
            "dataStreamName": "", 
            "dataType": {
                "field": [
                    {
                        "format": "integer", 
                        "name": "steps"
                    }
                ], 
                "name": "com.google.step_count.delta"
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

        response = json.loads(r.text)
        if response.get("error"):
            error_message = response["error"]["message"]
            raise Exception(error_message)
        else:     
            return response["dataStreamId"]
    
    def get_hr_data(self, datastream_id, time_from, time_to, transform = None):
        time_from = str(time_from) + '000000000'
        time_to = str(time_to) + '000000000'
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

            if transform:
                hr = transform(hr)

            data.append(hr)

        return data
    
    def get_step_data(self, datastream_id, time_from, time_to, transform = None):
        time_from = str(time_from) + '000000000'
        time_to = str(time_to) + '000000000'
        dataset_id = time_from + "-" + time_to

        r = self.client.get(
            "https://www.googleapis.com/fitness/v1/users/me/dataSources/"
            + datastream_id + "/datasets/" + dataset_id
        )

        # Parse data
        raw_data = json.loads(r.text)
        data = []
        for p in raw_data['point']:
            activity = {}
            activity['start_timestamp'] = int(p['startTimeNanos'][:-9])
            activity['end_timestamp'] = int(p['endTimeNanos'][:-9])
            activity['value'] = float(p['value'][0]['intVal'])

            if transform:
                activity = transform(activity)

            data.append(activity)

        return data

    def write_hr_value(self, value, datastream_id):
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
    
    def write_steps_value(self, value, datastream_id):
        now_ts = int((datetime.now() - datetime(1970,1,1)).total_seconds())
        
        end_ts = str(now_ts) + '000000000'
        start_ts = str(now_ts - 5 * 60) + '000000000'
        
        dataset_id = start_ts + "-" + end_ts

        headers = {'Accept' : 'application/json', 'Content-Type' : 'application/json'}
        data = {
            "minStartTimeNs": start_ts,
            "maxEndTimeNs": end_ts,
            "dataSourceId": datastream_id,
            "point": [
                {
                    "startTimeNanos": start_ts,
                    "endTimeNanos": end_ts,
                    "dataTypeName": "com.google.step_count.delta",
                    "originDataSourceId": datastream_id,
                    "value": [
                        {
                            "intVal": value, 
                            "mapVal": []
                        }
                    ],
                    "modifiedTimeMillis": start_ts[:-9]
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

    @staticmethod
    def get_refresh_token(client_id, client_secret):
        google = OAuth2Session(
            client_id,
            scope=GoogleFitClient.SCOPE,
            redirect_uri=GoogleFitClient.REDIRECT_URI
        )

        # Authorize
        authorization_url, state = google.authorization_url(
            GoogleFitClient.AUTHORIZATION_BASE_URL,
            access_type="offline",
            prompt="consent",
        )
        print 'Please go here and authorize,', authorization_url

        # Get the authorization verifier code from the callback url
        redirect_response = raw_input('Paste the redirect url here:')

        # Get the tokens
        r = google.fetch_token(
            GoogleFitClient.TOKEN_URL,
            client_secret=client_secret,
            authorization_response=redirect_response
        )

        return r['refresh_token']
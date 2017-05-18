# CAMI Store API
This document presents the set of Resources defining the RESTful
API that allows interaction with the core CAMI Store service.

The set of currently implemented endpoints allows to:
  * get the list of CAMI Users (no profile included yet)
  * get the list of Devices
  * get the list of DeviceUsage (device-user pairs) elements, which
    specify which device is used by which user and what are the
    __credentials__ used to retrieve data from _that_ device, for _that_ user
  * filter DeviceUsage entries by device_id and different keys from the access_info JSON (which
    describes data access means: e.g. Withings API token, consumer key, user_id)
  * get the list of Measurements (optionally ordered by timestamp)
  * filter the list of Measurements by: device_id, user_id, measurement type, timestamp,
  value and context information
  * insert a new measurement

## Endpoint details

#### List all Users:
```
http://<hostname_or_ip>:8000/api/v1/user/

Example output:
{
    meta: {
        limit: 20,
        next: null,
        offset: 0,
        previous: null,
        total_count: 4
    },
    users: [
        {
            date_joined: "2017-03-13T16:14:30.821835",
            devices: [
                "/api/v1/device/1/",
                "/api/v1/device/2/"
            ],
            first_name: "Cami",
            id: 14,
            is_active: true,
            last_login: null,
            last_name: "EndUser",
            resource_uri: "/api/v1/user/14/",
            username: "camidemo"
        },
        {
            date_joined: "2017-03-13T16:14:30.900014",
            devices: [
                "/api/v1/device/1/"
            ],
            first_name: "Cami",
            id: 15,
            is_active: true,
            last_login: null,
            last_name: "Caregiver",
            resource_uri: "/api/v1/user/15/",
            username: "camicare"
        }
    ]
}
```

<br/>

#### List all Devices:
```
http://<hostname_or_ip>:8000/api/v1/device/

Example output:
{
  "devices": [
    {
      "activation_date": "2017-03-09T13:11:22.689523",
      "created_at": "2017-03-09T13:11:22.689989",
      "device_type": "weight",
      "id": 1,
      "manufacturer": "Withings",
      "model": "WS 30",
      "resource_uri": "/api/v1/device/1/",
      "serial_number": "00:24:e4:24:6f:30",
      "updated_at": "2017-03-09T13:11:22.690069"
    },
    {
      "activation_date": "2017-03-09T13:11:22.704463",
      "created_at": "2017-03-09T13:11:22.704606",
      "device_type": "pulse",
      "id": 2,
      "manufacturer": "LG",
      "model": "Urbane",
      "resource_uri": "/api/v1/device/2/",
      "serial_number": "1234",
      "updated_at": "2017-03-09T13:11:22.704635"
    }
  ],
  "meta": {
    "limit": 20,
    "next": null,
    "offset": 0,
    "previous": null,
    "total_count": 2
  }
}
```
<br/>

#### List all DeviceUsage entries:
```
http://<hostname_or_ip>:8000/api/v1/deviceusage/

Example output:
{
  "meta": {
    "limit": 20,
    "next": null,
    "offset": 0,
    "previous": null,
    "total_count": 3
  },
  "objects": [
    {
      "access_info": {
        "withings_consumer_key": "5b1f8cbeb36cffe108fd8fdd666c51cb5d6eee9f2e2940983958b836451",
        "withings_consumer_secret": "2e75dfb7f1088f398b4cfc5ebed6d5909c48918ee637417e3b0de001b3b",
        "withings_measurement_type_id": "1",
        "withings_oauth_token": "59dd58ccbd19bfbd8b3522ce50d31c4cb6e530742d22234f4cb4bee11673084",
        "withings_oauth_token_secret": "cf31bc8e405d96b975b8014d93c722830bd55f44b437f27c7e6d5964b3",
        "withings_userid": "11115034"
      },
      "device": "/api/v1/device/1/",
      "id": 6,
      "resource_uri": "/api/v1/deviceusage/6/",
      "user": "/api/v1/user/14/",
      "uses_since": "2017-03-14"
    },
    {
      "access_info": {
        "google_fit_client_id": "701996606933-17j7km8f8ce8vohhdcnur453cbn44aau.apps.googleusercontent.com",
        "google_fit_client_secret": "K-lZ7t49-Gvhtz2P-RTqBhAQ",
        "google_fit_hr_datastream_id": "raw:com.google.heart_rate.bpm:com.ryansteckler.perfectcinch:",
        "google_fit_hr_test_datastream_name": "CAMI Heart Rate Test",
        "google_fit_refresh_token": "1/eAhtNXxq65LeyzTr4aju27wCPLDAipXdrEd8ovgO8CY",
        "google_fit_steps_test_datastream_name": "CAMI Steps Test"
      },
      "device": "/api/v1/device/2/",
      "id": 7,
      "resource_uri": "/api/v1/deviceusage/7/",
      "user": "/api/v1/user/14/",
      "uses_since": "2017-03-13"
    },
    {
      "access_info": {
        "withings_consumer_key": "5b1f8cbeb36cffe108fd8fdd666c51cb5d6eee9f2e2940983958b836451",
        "withings_consumer_secret": "2e75dfb7f1088f398b4cfc5ebed6d5909c48918ee637417e3b0de001b3b",
        "withings_measurement_type_id": "1",
        "withings_oauth_token": "59dd58ccbd19bfbd8b3522ce50d31c4cb6e530742d22234f4cb4bee11673084",
        "withings_oauth_token_secret": "cf31bc8e405d96b975b8014d93c722830bd55f44b437f27c7e6d5964b3",
        "withings_userid": "11115034"
      },
      "device": "/api/v1/device/1/",
      "id": 8,
      "resource_uri": "/api/v1/deviceusage/8/",
      "user": "/api/v1/user/15/",
      "uses_since": "2017-03-14"
    }
  ]
}
```
<br/>

#### Filter DeviceUsage by device_id and access_info related elements:
```
http://<hostname_or_ip>:8000/api/v1/deviceusage/?device=1&access_info__withings_userid=11115034
```
The `device` filter takes the integer ID value of the device resource, i.e. from a DeviceResource
URI such as `/api/v1/device/1/`, the ID is the last integer number `1`.

Access information filtering is done as per indications of [JSONField key-presence lookups](http://django-mysql.readthedocs.io/en/latest/model_fields/json_field.html#key-presence-lookups).

In the example given, `access_info` is the name of the `JSONField` holding the access information
for the WS 30 scale from Withings and `withings_userid` is the __key__ in the JSON dict holding
the userid value for the Withings account.

#### List and fitler Measurements:
```
http://<hostname_or_ip>:8000/api/v1/measurement/?order_by=-timestamp&device=1

Example output:
{
  "measurements": [
    {
      "context_info": {},
      "device": "/api/v1/device/1/",
      "id": 7,
      "measurement_type": "weight",
      "precision": 100,
      "resource_uri": "/api/v1/measurement/7/",
      "timestamp": "1489836002",
      "timezone": "GMT",
      "unit_type": "kg",
      "user": "/api/v1/user/14/",
      "value_info": {
        "value": 71.5
      }
    }
  ],
  "meta": {
    "limit": 20,
    "next": null,
    "offset": 0,
    "previous": null,
    "total_count": 1
  }
}
```


#### Create Measurement:
For measurement creation you have to send a POST request using a JSON payload to the `http://<hostname_or_ip>:8000/api/v1/measurement/` endpoint.
If successful, the endpoint will respond with a 201 (CREATED) status code and the JSON serialization of the newly created measurement.

Note that in the example payloads shown below, the foreign key elements (i.e. user and device for which
the measurement is taken) are given as relative URIs (as per Tastypie requirements): e.g. `/api/v1/user/14/`,
`/api/v1/device/2/`. The last integer number corresponds to the id of the entry in the DB (which is also
the primary key).

__Pulse__:
```
{
    "measurement_type": "pulse",
    "unit_type" : "bpm",

    "timestamp": 1489836002,
    "timezone": "UTC",

    "user": "/api/v1/user/14/",
    "device": "/api/v1/device/2/",
    "value_info": {
        "value": 75
    }
}
```

__Heart rate__:
```
{
    "measurement_type": "blood_pressure",
    "unit_type" : "mmhg",

    "timestamp": 1489836002,
    "timezone": "UTC",

    "user": "/api/v1/user/14/",
    "device": "/api/v1/device/2/",
    "value_info": {
        "systolic": 115,
        "diastolic": 70
    }
}
```

__Weight__:
```
{
    "measurement_type": "weight",
    "unit_type" : "kg",

    "timestamp": 1489836002,
    "timezone": "UTC",

    "user": "/api/v1/user/14/",
    "device": "/api/v1/device/1/",
    "value_info": {
        "value": 72
    }
}
```

#### Activities:

- full endpoint:
```
http://cami.vitaminsoftware.com:8008/api/v1/activity/

{
  "activities": [
    {
      "activity_type": "personal",
      "calendar_id": "7eh6qnivid6430dl79ei89k26g@group.calendar.google.com",
      "calendar_name": "Appointments",
      "color": "{u'foreground': u'#000000', u'id': u'16', u'background': u'#4986e7'}",
      "created": "1494544314",
      "creator": "{u'email': u'proiect.cami@gmail.com'}",
      "description": null,
      "end": "1494603000",
      "event_id": "pjm35ld41grfhrfh93472j1g24",
      "html_link": "https://www.google.com/calendar/event?eid=cGptMzVsZDQxZ3JmaHJmaDkzNDcyajFnMjQgN2VoNnFuaXZpZDY0MzBkbDc5ZWk4OWsyNmdAZw",
      "iCalUID": "pjm35ld41grfhrfh93472j1g24@google.com",
      "id": 1,
      "location": null,
      "recurring_event_id": null,
      "reminders": "{}",
      "resource_uri": "/api/v1/activity/1/",
      "start": "1494599400",
      "status": "confirmed",
      "title": "Tennis w/ Joel",
      "updated": "1494552610",
      "user": "/api/v1/user/2/"
    },
    {
      "activity_type": "personal",
      "calendar_id": "7eh6qnivid6430dl79ei89k26g@group.calendar.google.com",
      "calendar_name": "Appointments",
      "color": "{u'foreground': u'#000000', u'id': u'16', u'background': u'#4986e7'}",
      "created": "1494556425",
      "creator": "{u'email': u'proiect.cami@gmail.com'}",
      "description": null,
      "end": "1494689400",
      "event_id": "nd044tjlq84a3cl0hpcgu4ck6g",
      "html_link": "https://www.google.com/calendar/event?eid=bmQwNDR0amxxODRhM2NsMGhwY2d1NGNrNmcgN2VoNnFuaXZpZDY0MzBkbDc5ZWk4OWsyNmdAZw",
      "iCalUID": "nd044tjlq84a3cl0hpcgu4ck6g@google.com",
      "id": 2,
      "location": null,
      "recurring_event_id": null,
      "reminders": "{}",
      "resource_uri": "/api/v1/activity/2/",
      "start": "1494685800",
      "status": "confirmed",
      "title": "Footbal Game",
      "updated": "1494556425",
      "user": "/api/v1/user/2/"
    }
  ]
}
```

- last activities endpoint:

Currently, this retrieves the activities from 7 days in the past and 7 days in the future from the current time(14 days totally), for ALL the users. There is information only for one hardcoded user, though. This will have to be modified to accept a user parameter, in order to be able to retrieve information only for one user.

This is how it looks like:
```
http://cami.vitaminsoftware.com:8008/api/v1/activity/last_activities

[
  {
    "activity_type": "medication",
    "calendar_id": "us8v5j6ttp885542q9o2aljrho@group.calendar.google.com",
    "calendar_name": "Medication",
    "color": {
      "background": "#fa573c",
      "foreground": "#000000",
      "id": "4"
    },
    "created": 1494413636,
    "creator": {
      "email": "proiect.cami@gmail.com"
    },
    "description": null,
    "end": 1494504000,
    "event_id": "h5bk4mchboa9od1uroe93nthi8_20170511T110000Z",
    "html_link": "https://www.google.com/calendar/event?eid=aDViazRtY2hib2E5b2QxdXJvZTkzbnRoaThfMjAxNzA1MTFUMTEwMDAwWiB1czh2NWo2dHRwODg1NTQycTlvMmFsanJob0Bn",
    "iCalUID": "h5bk4mchboa9od1uroe93nthi8@google.com",
    "id": 6,
    "location": null,
    "recurring_event_id": "h5bk4mchboa9od1uroe93nthi8",
    "reminders": {},
    "start": 1494500400,
    "status": "confirmed",
    "title": "Analgezics",
    "updated": 1494413636,
    "user_id": 2
  }
]
```

- endpoint for manual synchronization of the events from GCal:
A simple GET request from the browser address bar or CURL will trigger the syncronization.

`http://cami.vitaminsoftware.com:8008/sync_activities/`
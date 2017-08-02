Store container
============
This is the container that holds the central API of CAMI System. This API is responsible for inserting and delivering all kinds of data from CAMI Cloud DB, like measurements, journal entries, activities or users.

## API Documentation
The documentation for this API is automatically generated using the `django-tastypie-swagger` Django plugin. The output is Swagger 1.0.

It can be found at: [http://cami.vitaminsoftware.com:8008/api/documentation/](http://cami.vitaminsoftware.com:8008/api/documentation/)

## Activities
There is one endpoint that does not appear in the auto-generated documentation, but it is worth mentioning:

**Last activities endpoint**

Currently, this retrieves the activities from 7 days in the past and 7 days in the future from the current time(14 days totally), for ALL the users. There is information only for one hardcoded user, though. This will have to be modified to accept a user parameter, in order to be able to retrieve information only for one user.

The elements of the array are actually `activity` objects, that are described here: [http://cami.vitaminsoftware.com:8008/api/documentation/#!/activity/activity_detail](http://cami.vitaminsoftware.com:8008/api/documentation/#!/activity/activity_detail)

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
Google Calendar container
====================

This is the container that handles the integration with the Google Calendar API. It is responsible for fetching calendar events for each user in the system. 

It can be run as:
  - `Celery beat`: for running the main sync task on a given schedule
  - `Celery worker`: for processing sync (sub-)tasks
 
This can be scaled up:
  - the main sync task sends sub-tasks to the queue for each user from the system
  - there can be multiple Google Calendar workers that can process each sub-task in part 

It's currently set to sync events once per 5 minutes. This can be changed in the settings file.

The events are fetched on a time frame of `14 days`, `7 days` in the past and `7 days` in the future from the moment the sync is triggered. Currently, this time frame is hardcoded in the `activities.py` file.

The fetched events are then stored in the database through the `store` container, using it's `Store API`. Currently, the functions that are used for communicating with the `Store API` can be found in the `store_utils.py` file.

Container's file list:
- `activities.py`
- `google_calendar_backend.py`
- `client_secret.json`
- `cami-calendar-quickstart.json`
- `requirements.txt`
- `run_celery.py`
- `run_celery_beat.py`
- `settings.py`
- `store_utils.py`
- `tasks.py`

## Description of the files
### `activities.py`
This file contains the main functionality, the engine of this container.

- `sync_for_user`

This is the main function that is called from the `task.py` file. 
It computes the time frame and it fetches the user's calendars for the four types of activities.
Then, for each calendar, it fetches it's events using Google API and sends them to `process_events`.

- `process_events`

This function gets the already stored activities from DB and compares them with the just fetched ones from Google API. It then inserts the new ones, that are not already in DB, updates the ones that are already in DB but have changed, and deletes the ones that do not exist anymore in the Google Calendar.

- `compose_activity_data`

This is the function that creates the payload to be sent to the `Store API` for inserting/updating an activity.

- `event_equals_activity`

This is the function used in `process_events` for comparing an event from Google API with an activity from database, in order to check if the activity should be updated.

- `timestamp_from_event_date`

This translates a date from Google API format to a Unix timestamp

### `google_calendar_backend.py`
This contains some wrapper functions over the Google Calendar API calls.

### `client_secret.json`
This is the file that stores the Google Calendar API key.

### `cami-calendar-quickstart.json`
This is the file that stores the current OAuth2 tokens to be used with the Google Calendar API. It is currently hardcoded for test CAMI Google Calendar account, and it works out-of-the-box, no setup steps are needed right now.

This tokens should actually be stored in the database, for each user in part, but we'll handle this when we will add multi-user support.

### `requirements.txt`
This is the Python package requirements file.

### `run_celery.py`
This file is used as Celery **worker** entrypoint when building for **development**. It starts the Celery **worker** with support for restarting the worker when file changes in the current directory are detected. It only keeps track of the changes made to `.py` files.

### `run_celery_beat.py`
This file is used as Celery **beat** entrypoint when building for **development**. It starts the Celery **beat** with support for restarting the worker when file changes in the current directory are detected. It only keeps track of the changes made to `.py` files.

### `settings.py`
This is the file that keeps the container's settings. The settings that can be made at the moment are:
- Sentry (API key)
- Papertrail (hostname, port)
- Logging (level, handlers, etc.)
- Celery Beat Schedule (task, period)
- Celery (url, queues)
- Store API (hostname, port)

### `store_utils.py`
This file contains some wrapper functions over the `Store Tastypie API`. More info about the endpoints used here can be found in the `store` container documentation.

Currently implemented functions:
- `activity_get`

Function for getting a list of activities from the `store`.
**Arguments**: Django Query-like arguments (https://docs.djangoproject.com/en/1.11/topics/db/queries/#retrieving-objects)
**Returns**: an array of Activities represented as Python Dictionaries
**Example**:
```
activities = store_utils.activity_get(
    start__gte=int(time.mktime(date_from.timetuple())),
    end__lte=int(time.mktime(date_to.timetuple())),
    user=user['id'],
    calendar_id=calendar['id']
)
```

- `activity_save`

Function for saving an activity to the `store`. It works mostly like the constructor function used for creating entries directly from a Django Model. If a valid id is provided, then the already existent entry will be updated using `PUT` method on the Tastypie endpoint, otherwise a `POST` request is issued for creating a new entry.
**Arguments**:  Django Query-like arguments (https://docs.djangoproject.com/en/1.11/topics/db/queries/#creating-objects)
**Returns**: `True` if the activity was successfully saved; `False` otherwise
**Example**:
```
store_utils.activity_save(
    status="confirmed",
    html_link="http://example.com",
    title="Coffee with Joel"
)
```

- `activity_delete`

Function for deleting activities from the `store`.
**Arguments**: Django Query-like arguments (https://docs.djangoproject.com/en/1.11/topics/db/queries/#deleting-objects). **WARNING**: If no argument is provided, **all** the entries will be deleted. The same goes if an argument with an empty value is provided.
**Returns**: `True` if the activities were successfully deleted; `False` otherwise
**Example**:
```
store_utils.activity_delete(id__in=[24, 32, 46])
```

- `user_get`

Function for getting an user's details by its`id`.
**Arguments**: User's unique, numeric **id**.
**Returns**: The user's details as a Python Dictionary
**Example**:
```
user = store_utils.user_get(2)
```

### `tasks.py`
This file contains the Celery tasks that can be called over the Celery queue.
Currently implemented tasks:
- `sync_activities`

This is the main task, that will spawn a sync sub-task for each user from the system.
Currently, there is only one user hardcoded in this method.

- `sync_activities_for_user`

This is the sub-task that will call the `sync_for_user` function from `activities.py` for the user received as a parameter through the queue.
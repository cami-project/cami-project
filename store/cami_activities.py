from __future__ import print_function
import httplib2
import os

from apiclient import discovery
from oauth2client import client
from oauth2client import tools
from oauth2client.file import Storage
from apiclient import errors

from store.constants import ActivityType, ActivitySource

import datetime, pytz, json, dateutil.tz
import dateutil.rrule as recrule
import dateutil.parser

try:
    import argparse
    flags = argparse.ArgumentParser(parents=[tools.argparser]).parse_args()
except ImportError:
    flags = None

# If modifying these scopes, delete your previously saved credentials
# at ~/.credentials/cami-calendar-quickstart.json
SCOPES = 'https://www.googleapis.com/auth/calendar'
CLIENT_SECRET_FILE = 'client_secret.json'
APPLICATION_NAME = 'CAMI Google Calendar API'

ACTIVITY_TYPE   = "activity_type"
ACTIVITY_SOURCE = "activity_source"

CARETAKER_1_CAL_ID = "g822rl14osjc75mf88lovci6kg@group.calendar.google.com"
CARETAKER_2_CAL_ID = "68d502bqtor99dfpast08bapfk@group.calendar.google.com"

def get_credentials():
    """Gets valid user credentials from storage.

    If nothing has been stored, or if the stored credentials are invalid,
    the OAuth2 flow is completed to obtain the new credentials.

    Returns:
        Credentials, the obtained credential.
    """
    home_dir = os.path.expanduser('~')
    credential_dir = os.path.join(home_dir, '.credentials')
    if not os.path.exists(credential_dir):
        os.makedirs(credential_dir)
    credential_path = os.path.join(credential_dir,
                                   'cami-calendar-quickstart.json')

    store = Storage(credential_path)
    credentials = store.get()
    if not credentials or credentials.invalid:
        flow = client.flow_from_clientsecrets(CLIENT_SECRET_FILE, SCOPES)
        flow.user_agent = APPLICATION_NAME
        if flags:
            credentials = tools.run_flow(flow, store, flags)
        else: # Needed only for compatibility with Python 2.6
            credentials = tools.run(flow, store)
        print('Storing credentials to ' + credential_path)
    return credentials


def get_calendar_service(credentials):
    http = credentials.authorize(httplib2.Http())
    service = discovery.build('calendar', 'v3', http=http)

    return service

""" ============================= CREATE ACTIVITIES ============================= """
def create_activity(calendar_service, calendar_id,
                    title, description = None,
                    start_date = None, end_date = None, timezone_spec = "Europe/Bucharest",
                    activity_type = ActivityType.PERSONAL, activity_source = ActivitySource.SELF,
                    recurrence_rule = None):
    """
    Creates a (possibly) recurrent activity.
    :param calendar_service:
    :param calendar_id:
    :param title:
    :param description:
    :param start_date:
    :param end_date:
    :param timezone_spec
    :param activity_type:
    :param activity_source:
    :param recurrence_rule:
    :return: A tuple of type (created_obj, status_code) or (None, status_code) if the operation fails.
    """

    if start_date is None:
        start_date = datetime.datetime.now(tz=dateutil.tz.tzoffset('GMT', 7200))

    # if no end_date is provided, the default is for the activity to last during the whole day
    if end_date is None:
        d = start_date.date()
        next_d = d + datetime.timedelta(days=1)
        end_date = datetime.datetime(next_d.year, next_d.month, next_d.day, 0, 0, 0, 0, tzinfo=start_date.tzinfo)

    body = {
        "summary": title,

        "start": {
            'dateTime': start_date.isoformat(),
            'timeZone': timezone_spec
        },
        "end": {
            'dateTime': end_date.isoformat(),
            'timeZone': timezone_spec
        },

        "extendedProperties": {
            "private": {
                'activity_type': activity_type,
                'activity_source': activity_source
            }
        },

        "visibility": "private"
    }

    if description:
        body['description'] = description

    if recurrence_rule:
        body['recurrence'] = [recurrence_rule]

    req = calendar_service.events().insert(calendarId=calendar_id, body=body)

    try:
        res = req.execute()
        return res, 200
    except errors.HttpError as e:
        print(e)
        return None, e.resp.status


def create_single_activity(calendar_service, calendar_id,
                           title, description = None,
                           start_date = None, end_date = None, timezone_spec = "Europe/Bucharest",
                           activity_type = ActivityType.PERSONAL, activity_source = ActivitySource.SELF):
    return create_activity(calendar_service, calendar_id, title, description=description,
                           start_date=start_date, end_date = end_date, timezone_spec=timezone_spec,
                           activity_type=activity_type, activity_source=activity_source)


def create_weekly_activity(calendar_service, calendar_id,
                           title, description = None,
                           start_date = None, end_date = None, timezone_spec = "Europe/Bucharest",
                           activity_type = ActivityType.PERSONAL, activity_source = ActivitySource.SELF,
                           interval = 1, nr_instances = None, until_date = None, specific_weekdays = None
                           ):

    ## prepare recurrence rule
    if not nr_instances and not until_date:
        return ValueError("create_weekly_activity method must specify either `nr_instances` or `until_date` parameters.")

    recurrence = recrule.rrule(recrule.WEEKLY, interval=interval)

    if nr_instances:
        recurrence = recurrence.replace(count = nr_instances)
    elif until_date:
        recurrence = recurrence.replace(until = until_date)

    if specific_weekdays:
        if isinstance(specific_weekdays, tuple):
            recurrence = recurrence.replace(byweekday=specific_weekdays)
        else:
            return ValueError("The `specific_weekdays` parameter must be a tuple.")

    # TODO do this more nicely
    # hack to disable inclusion of DTSART output in string format of recurrence rule
    recurrence._dtstart = None

    rec_str = "RRULE:" + str(recurrence)
    # print(rec_str)

    return create_activity(calendar_service, calendar_id, title, description=description,
                           start_date=start_date, end_date=end_date, timezone_spec=timezone_spec,
                           activity_type=activity_type, activity_source=activity_source,
                           recurrence_rule=rec_str)


def create_daily_activity(calendar_service, calendar_id,
                           title, description = None,
                           start_date = None, end_date = None, timezone_spec = "Europe/Bucharest",
                           activity_type = ActivityType.PERSONAL, activity_source = ActivitySource.SELF,
                           interval = 1, nr_instances = None, until_date = None, specific_hours = None
                           ):

    ## prepare recurrence rule
    if not nr_instances and not until_date:
        return ValueError(
            "create_daily_activity method must specify either `nr_instances` or `until_date` parameters.")

    recurrence = recrule.rrule(recrule.DAILY, interval=interval)

    if nr_instances:
        recurrence = recurrence.replace(count = nr_instances)
    elif until_date:
        recurrence = recurrence.replace(until = until_date)

    if specific_hours:
        if isinstance(specific_hours, tuple):
            recurrence = recurrence.replace(byhour=specific_hours)
        else:
            return ValueError("The `specific_hours` parameter must be a tuple.")

    # TODO do this more nicely
    # hack to disable inclusion of DTSART output in string format of recurrence rule
    recurrence._dtstart = None

    rec_str = "RRULE:" + str(recurrence)
    # print(rec_str)

    return create_activity(calendar_service, calendar_id, title, description=description,
                           start_date=start_date, end_date=end_date, timezone_spec=timezone_spec,
                           activity_type=activity_type, activity_source=activity_source,
                           recurrence_rule=rec_str)


""" ============================= GET ACTIVITIES ============================= """
def list_activities(calendar_service, calendar_id, start_date = None, end_date = None, activity_type = None, activity_source = None):
    """
    Returns the activities of a caretaker identified by `calendar_id`. Can filter by start and end times, activity_type (see :class:`store.models.Activity`) and
    activity_source (see :class:`store.models.Activity`)
    The API call uses single_events = True to get individual instances of activities defined as a recurring event.
    :param calendar_service:
    :param calendar_id:
    :param start_date:
    :param end_date:
    :param activity_type:
    :param activity_source:
    :return:
    """

    time_min_str = None
    time_max_str = None
    private_props = []

    if start_date:
        time_min_str = start_date.strftime("%Y-%m-%dT%H:%M:%S%z")

    if end_date:
        time_max_str = end_date.strftime("%Y-%m-%dT%H:%M:%S%z")

    if activity_type:
        private_props.append(ACTIVITY_TYPE + "=" + activity_type)

    if activity_source:
        private_props.append(ACTIVITY_SOURCE + "=" + activity_source)

    req = calendar_service.events().list(calendarId=calendar_id,
                                         privateExtendedProperty=private_props,
                                         timeMin=time_min_str, timeMax=time_max_str,
                                         singleEvents=True,
                                         orderBy="startTime")
    # print(req.uri)

    event_results = consume_event_results(calendar_service, req)

    return event_results


def consume_event_results(calendar_service, api_req):
    event_results = []

    while True:
        try:
            current_res = api_req.execute()
        except errors.HttpError as e:
            print(e)
            break

        if current_res['items']:
            event_results.extend(current_res['items'])

        api_req = calendar_service.events().list_next(api_req, current_res)
        if not api_req:
            break

    return event_results


""" ============================= MODIFY ACTIVITIES ============================= """
def get_activity(calendar_service, calendar_id, activity_id):
    """
    Returns a python dict corresponding to the JSON description of a GCal Event
    :param calendar_service:
    :param calendar_id:
    :param activity_id:
    :return:
    """
    req = calendar_service.events().get(calendarId=calendar_id, eventId=activity_id)

    try:
        res = req.execute()
        return res, 200
    except errors.HttpError as e:
        print(e)
        return None, e.resp.status



def postpone_activity(calendar_service, calendar_id, activity_data = None, new_start_date = None, new_end_date = None, timezone = "Europe/Bucharest"):
    """
    Postpones the activity whose dict data is given in `activity_data` to take place between `new_start_date` and `new_end_date`.
    This function call modified a single instance, not to a whole recurrence list.
    :param calendar_service:
    :param calendar_id:
    :param activity_data:
    :param new_start_date:
    :param new_end_date:
    :param timezone:
    :return:
    """
    if not activity_data:
        return ValueError("`activity_data` parameter must be provided for postpone_activity function.")

    if not new_start_date or not new_end_date:
        return ValueError("`new_start_date` and `new_end_date` parameters must be provided for postpone_activity functions.")

    # update activity_data
    new_data = activity_data.copy()

    new_data['start'] = {
        'dateTime': new_start_date.isoformat(),
        'timeZone': timezone
    }

    new_data['end'] = {
        'dateTime': new_end_date.isoformat(),
        'timeZone': timezone
    }

    req = calendar_service.events().update(calendarId=calendar_id, eventId=activity_data['id'], body=new_data)

    try:
        res = req.execute()
        return res, 200
    except errors.HttpError as e:
        print(e)
        return None, e.resp.status


def cancel_activity(calendar_service, calendar_id, activity_id):
    """
    Cancel an activity identified by activity_id.
    :param calendar_service:
    :param calendar_id:
    :param activity_id:
    :return:
    """
    req = calendar_service.events().delete(calendarId=calendar_id, eventId=activity_id)

    try:
        res = req.execute()
        return res, 200
    except errors.HttpError as e:
        print(e)
        return None, e.resp.status


if __name__=="__main__":
    credentials = get_credentials()
    calendar_service = get_calendar_service(credentials)

    # TEST RETRIEVAL OF ACTIVITIES
    # calendar_id = CARETAKER_1_CAL_ID
    # start_date = datetime.datetime(2017,1,27,10,0,0,0, tzinfo=dateutil.tz.tzoffset('GMT', 7200))
    # end_date = datetime.datetime(2017,1,27,13,0,0,0, tzinfo=dateutil.tz.tzoffset('GMT', 7200))
    #
    # activity_type = "medication"
    # activity_source = "self"
    #
    # event_list = list_activities(calendar_service, calendar_id, start_date=start_date, end_date=end_date, activity_type=None, activity_source=activity_source)
    #
    # if event_list:
    #     for ev in event_list:
    #         print (json.dumps(ev, indent=4))
    # else:
    #     print("No events matching criteria.")

    # TEST CREATE SINGLE ACTIVITY
    # title1 = "X-Body Workout"
    # start_date1 = datetime.datetime(2017,1,27, 11, 0, 0, 0, tzinfo=dateutil.tz.tzoffset('GMT', 7200))
    # end_date1 = datetime.datetime(2017,1,27, 12, 0, 0, 0, tzinfo=dateutil.tz.tzoffset('GMT', 7200))
    #
    # title2 = "Take Muscle Pills"
    # start_date2 = datetime.datetime(2017, 1, 27, 10, 30, 0, 0, tzinfo=dateutil.tz.tzoffset('GMT', 7200))
    # end_date2 = datetime.datetime(2017, 1, 27, 11, 0, 0, 0, tzinfo=dateutil.tz.tzoffset('GMT', 7200))
    #
    # # Create Event 1
    # event1, status_code = create_single_activity(calendar_service, CARETAKER_1_CAL_ID, title=title1,
    #                                             start_date = start_date1, end_date = end_date1,
    #                                             activity_type = ActivityType.EXERCISE,
    #                                             activity_source = ActivitySource.SELF)
    # if event1:
    #     print(json.dumps(event1, indent=4))
    # else:
    #     print("Operation failed for event 1! Status code: " + str(status_code))
    #
    # # Create Event 2
    # event2, status_code = create_single_activity(calendar_service, CARETAKER_1_CAL_ID, title=title2,
    #                                              start_date=start_date2, end_date=end_date2,
    #                                              activity_type=ActivityType.MEDICATION,
    #                                              activity_source=ActivitySource.SELF)
    #
    # if event2:
    #     print(json.dumps(event2, indent=4))
    # else:
    #     print("Operation failed for event 2! Status code: " + str(status_code))

    # TEST CREATE RECURRENT ACTIVITY
    # title = "X-Body Workout"
    # # start_date = datetime.datetime(2017,1,27, 11, 0, 0, 0, tzinfo=dateutil.tz.tzoffset('GMT', 7200))
    # # end_date = datetime.datetime(2017,1,27, 12, 0, 0, 0, tzinfo=dateutil.tz.tzoffset('GMT', 7200))
    #
    # start_date = datetime.datetime(2017,1,27, 11, 0, 0, 0)
    # end_date = datetime.datetime(2017,1,27, 12, 0, 0, 0)
    #
    # interval = 1
    # nr_instances = 3
    # specific_weekdays = (recrule.MO, recrule.TH)
    #
    # event, status = create_weekly_activity(calendar_service, CARETAKER_1_CAL_ID, title,
    #                                         start_date=start_date, end_date=end_date, timezone_spec="Europe/Bucharest",
    #                                         activity_type=ActivityType.MEDICATION,
    #                                         activity_source=ActivitySource.SELF,
    #                                         interval=interval, nr_instances=nr_instances, specific_weekdays=specific_weekdays)
    #
    # if status == 200:
    #     print ("Recurring activity created successfully!")
    # else:
    #     print ("Recurring activity creation FAILED!")


    ## TEST GET AND MODIFY ACTIVITY
    activity_id = "3ahieehjnmps8apsnsq968e9ms_20170202T090000Z"

    activity_data, status = get_activity(calendar_service, CARETAKER_1_CAL_ID, activity_id)

    new_start_date = datetime.datetime(2017,2,2, 17, 0, 0, 0)
    new_end_date = datetime.datetime(2017,2,2, 18, 0, 0, 0)

    new_activity_data, status = postpone_activity(calendar_service, CARETAKER_1_CAL_ID, activity_data=activity_data,
                                                  new_start_date=new_start_date, new_end_date=new_end_date)

    if status == 200:
        print ("Activity postponed successfully!")
    else:
        print ("Activity postponing FAILED!")

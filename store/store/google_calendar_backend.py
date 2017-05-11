import os
import logging
import httplib2
import datetime

from apiclient import errors, discovery
from oauth2client import tools, client
from oauth2client.file import Storage

from .constants import ActivityType, ActivitySource


SCOPES = 'https://www.googleapis.com/auth/calendar'
CLIENT_SECRET_FILE = 'client_secret.json'
CREDENTIALS_FILE = 'cami-calendar-quickstart.json'
APPLICATION_NAME = 'CAMI Google Calendar API'


logger = logging.getLogger("store")

def get_credentials():
    """Gets valid user credentials from storage.

    If nothing has been stored, or if the stored credentials are invalid,
    the OAuth2 flow is completed to obtain the new credentials.

    Returns:
        Credentials, the obtained credential.
    """

    project_dir = os.path.dirname(os.path.realpath(__file__)) + "/../"
    credential_dir = project_dir
    if not os.path.exists(credential_dir):
        os.makedirs(credential_dir)
    credential_path = os.path.join(credential_dir, CREDENTIALS_FILE)

    store = Storage(credential_path)
    credentials = store.get()

    if not credentials or credentials.invalid:
        client_secret_file_path = os.path.join(project_dir, CLIENT_SECRET_FILE)
        flow = client.flow_from_clientsecrets(client_secret_file_path, SCOPES)
        flow.user_agent = APPLICATION_NAME

        flags = tools.argparser.parse_args(args=[])
        credentials = tools.run_flow(flow, store, flags)

        logger.info('Storing GCal acccess credentials to ' + credential_path)

    return credentials

def get_calendar_service(credentials):
    http = credentials.authorize(httplib2.Http())
    service = discovery.build('calendar', 'v3', http=http)

    return service

def list_activities(calendar_service, calendar_id, start_date = None, end_date = None):
    """
    Returns the activities of a caretaker identified by `calendar_id`. Can filter by start and end times.
    The API call uses single_events = True to get individual instances of activities defined as a recurring event.
    :param calendar_service:
    :param calendar_id:
    :param start_date:
    :param end_date:
    :return:
    """

    time_min_str = None
    time_max_str = None
    private_props = []

    if start_date:
        time_min_str = start_date.strftime("%Y-%m-%dT%H:%M:%S%z")

    if end_date:
        time_max_str = end_date.strftime("%Y-%m-%dT%H:%M:%S%z")

    req = calendar_service.events().list(
        calendarId=calendar_id,
        privateExtendedProperty=private_props,
        timeMin=time_min_str,
        timeMax=time_max_str,
        singleEvents=True,
        orderBy="startTime"
    )

    return consume_event_results(calendar_service, req)

def get_calendar(calendar_service, calendar_id):
    """
    Returns a python dict corresponding to the JSON description of a GCal
    :param calendar_service:
    :param calendar_id:
    :return:
    """
    req = calendar_service.calendarList().get(calendarId=calendar_id)

    try:
        res = req.execute()
        return res, 200
    except errors.HttpError as e:
        logger.exception("[google_calendar_backend] Cannot retrieve remote calendar: %s" % calendar_id)
        raise e

def consume_event_results(calendar_service, api_req):
    event_results = []

    while True:
        try:
            current_res = api_req.execute()
        except errors.HttpError as e:
            logger.exception("[google_calendar_backend] Cannot consume all results from api_req %s." % (api_req))
            break

        if current_res['items']:
            event_results.extend(current_res['items'])

        api_req = calendar_service.events().list_next(api_req, current_res)
        if not api_req:
            break

    return event_results


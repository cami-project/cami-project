"""
Holds definitions for CAMI device, activity and measurement data
"""

ACTIVITY_TYPE       = "activity_type"
ACTIVITY_SOURCE     = "activity_source"
ACTIVITY_LOCAL_ID   = "activity_local_id"

CALENDAR_BACKEND_SERVICE    = "Google Calendar Backend"
CALENDAR_BACKEND_URI        = "https://www.googleapis.com/calendar/v3/calendars"

class ActivityType(object):
    PERSONAL = "personal"
    EXERCISE = "exercise"
    MEDICATION = "medication"
    MEASUREMENT = "measurement"

class ActivitySource(object):
    SELF = "self"
    DOCTOR = "doctor"
    CAREGIVERR = "caregiver"
    RECOMMENDATION = "recommendation"
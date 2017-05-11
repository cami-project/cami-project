"""
Holds definitions for CAMI device, activity and measurement data
"""

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

class MeasurementType(object):
    WEIGHT = "weight",
    BLOOD_PRESSURE = "blood_pressure",
    PULSE = "pulse",
    SATURATION = "saturation",
    STEPS = "steps"

class DeviceType(object):
    WEIGHT = "weight",
    BLOOD_PRESSURE = "blood_pressure",
    PULSE = "pulse",
    OXYMETER = "oxymeter",
    PEDOMETER = "pedometer"
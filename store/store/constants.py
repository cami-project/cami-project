"""
Holds definitions for CAMI device, activity and measurement data
"""

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
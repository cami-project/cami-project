import os
import raven

from kombu import Exchange, Queue
from datetime import timedelta

# Sentry integration
RAVEN_CONFIG = {
    # Set the Sentry API key here
    'dsn': 'https://d9bec7e9f54943a281d5271c29932e7c:b57cfbbc5edc456aa2ece299cabbd785@sentry.io/104123',
    'release': raven.fetch_git_sha(os.path.dirname(__file__) + "/../")
}

PAPERTRAILS_LOGGING_HOSTNAME = 'logs4.papertrailapp.com'
PAPERTRAILS_LOGGING_PORT = 43843

LOGGING = {
    'version': 1,
    'disable_existing_loggers': True,
    'formatters': {
        'verbose': {
            'format': '%(levelname)s %(asctime)s %(module)s '
                      '%(process)d %(thread)d %(message)s'
        },
    },
    'handlers': {
        'sentry': {
            'level': 'ERROR', # Set the Sentry logging level here
            'class': 'raven.handlers.logging.SentryHandler',
            'dsn': RAVEN_CONFIG['dsn'],
        },
        'console': {
            'level': 'DEBUG',
            'class': 'logging.StreamHandler',
            'formatter': 'verbose'
        },
        'file': {
            'level': 'DEBUG',
            'class': 'logging.FileHandler',
            'filename': './debug.log',
        },
        'syslog': {
            'level':'DEBUG',
            'class':'logging.handlers.SysLogHandler',
            'formatter': 'verbose',
            'address':(PAPERTRAILS_LOGGING_HOSTNAME, PAPERTRAILS_LOGGING_PORT)
        },
    },
    'loggers': {
        'google_calendar': {
            'level': 'DEBUG',
            'handlers': ['sentry', 'console', 'syslog', 'file'],
        },
        'raven': {
            'level': 'DEBUG',
            'handlers': ['console'],
            'propagate': False,
        },
        'sentry.errors': {
            'level': 'DEBUG',
            'handlers': ['console'],
            'propagate': False,
        },
    }
}

## Sync Google Calendar Events Scheduled Task
CELERYBEAT_SCHEDULE = {
    'sync_activities': {
        'task': 'google_calendar.sync_activities',
        'schedule': timedelta(minutes=5),
    },
    'process_reminders': {
        'task': 'google_calendar.process_reminders',
        'schedule': timedelta(minutes=1),
    },
}

## CELERY settings
BROKER_URL = 'amqp://cami:cami@cami-rabbitmq:5672/cami'

CELERY_DEFAULT_QUEUE = 'google_calendar'
CELERY_QUEUES = (
    Queue('google_calendar', Exchange('google_calendar'), routing_key='google_calendar'),
)

## STORE settings
STORE_HOST = "cami-store"
STORE_PORT = "8008"
STORE_ENDPOINT_URI = "http://" + STORE_HOST + ":" + STORE_PORT

CAMI_DEMO_USER_ID = 2
TRIAL_USER_DK_ID = 5
TRIAL_USER_PL_ID = 6
TRIAL_USER_RO_ID = 7



## Hardcoded calendar IDs for each user ID in the field trial
CALENDAR_IDs = {
    # cami demo
    CAMI_DEMO_USER_ID : {
        "personal": "7eh6qnivid6430dl79ei89k26g@group.calendar.google.com",
        "exercise": "8puar0sc4e7efns5r849rn0lus@group.calendar.google.com",
        "medication": "us8v5j6ttp885542q9o2aljrho@group.calendar.google.com"
    },
    # trial user DK
    TRIAL_USER_DK_ID : {
        "personal": "ai8jdd4j5tnrkpg4pdh5evqi7s@group.calendar.google.com",
        "exercise": "nh3uiqj91onn8nf8id9g2vl40g@group.calendar.google.com",
        "medication": "63peuih1pd85qf4hk5tj8jpktg@group.calendar.google.com"
    },
    # trial user PL
    TRIAL_USER_PL_ID : {
        "personal": "v8mkc24kpui19llr3gs9s515jk@group.calendar.google.com",
        "exercise": "0gvlkcke3jeqm5f0otatf83fvg@group.calendar.google.com",
        "medication": "j76o5e59ufmn9bh1ggjokh261c@group.calendar.google.com"
    },
    # trial user RO
    TRIAL_USER_RO_ID : {
        "personal": "oiu2qp85kvjegp9dgiqg9bsp08@group.calendar.google.com",
        "exercise": "66jp8d442fom00odg27jhvudg4@group.calendar.google.com",
        "medication": "5splak5bt78vd9n27qe3irsgv0@group.calendar.google.com"
    }
}

CALENDAR_CREDENTIALS = {
    # cami demo
    CAMI_DEMO_USER_ID : {
        "client_secret_file": 'client_secret.json',
        "credentials_file": "cami-calendar-quickstart.json"
    },
    # trial user DK
    TRIAL_USER_DK_ID : {
        "client_secret_file": 'client_secret_dk.json',
        "credentials_file": "cami-calendar-quickstart-dk.json"
    },
    # trial user PL
    TRIAL_USER_PL_ID : {
        "client_secret_file": 'client_secret_pl.json',
        "credentials_file": "cami-calendar-quickstart-pl.json"
    },
    # trial user RO
    TRIAL_USER_RO_ID : {
        "client_secret_file": 'client_secret_ro.json',
        "credentials_file": "cami-calendar-quickstart-ro.json"
    }
}

try:
    from settings_local import *
except:
    pass

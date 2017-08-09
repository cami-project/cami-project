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


try:
    from settings_local import *
except:
    pass

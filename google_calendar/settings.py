import os
import raven

from datetime import timedelta
from kombu import Exchange, Queue

## Sync Google Calendar Events Scheduled Task
CELERYBEAT_SCHEDULE = {
    'sync_activities': {
        'task': 'store.sync_activities',
        'schedule': timedelta(minutes=5),
    },
}

## CELERY settings
BROKER_URL = 'amqp://cami:cami@cami-rabbitmq:5672/cami'

CELERY_DEFAULT_QUEUE = 'store_activities_sync'
CELERY_QUEUES = (
    Queue('store_activities_sync', Exchange('store_activities_sync'), routing_key='store_activities_sync'),
)

try:
    from settings_local import *
except:
    pass
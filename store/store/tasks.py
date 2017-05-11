import logging

from kombu import Queue, Exchange
from celery import Celery

from django.conf import settings

import activities
from .models import User

logger = logging.getLogger("store")

app = Celery('store', broker=settings.BROKER_URL)
app.config_from_object('django.conf:settings')


@app.task(name='store.sync_activities')
def sync_activities():
    logger.debug("[sync-activities] Synchronizing all users activities with Google Calendar. Number of users: %d" % 1)

    # Hardcode user for demo
    user = User.objects.get(username="camidemo")
    app.send_task('store.sync_activities_for_user', [user])

    logger.debug("[sync-activities] Finished synchronizing all users activities with Google Calendar!")

@app.task(name='store.sync_activities_for_user')
def sync_activities_for_user(user):
    activities.sync_for_user(user)

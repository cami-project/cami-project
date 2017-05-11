from __future__ import absolute_import

import os
from celery import Celery
from celery.utils.log import get_task_logger

# set the default Django settings module for the 'celery' program.
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'store.settings')
from django.conf import settings #noqa

logger = get_task_logger('store.activity_sync_tasks')

app = Celery('store', broker=settings.BROKER_URL)
app.config_from_object('django.conf:settings')
app.autodiscover_tasks(lambda: settings.INSTALLED_APPS)

from kombu import Queue, Exchange
from .gcal_activity_backend import *
from .models import EndUserProfile, Activity, User
import dateutil.parser
import pytz
import datetime

def test_model():
    user = User.objects.get(username="camidemo")
    x = Activity(
        event_id = "test1",
        user = user,
        status = "test",
        html_link = "http://test",
        title = "test title",
        description = "Test description",
        calendar_id = "test calendar id",
        calendar_name = "test calendar name",
        start = 1494490000,
        end = 1494496959,
        created = 1494496959,
        updated = 1494496959
    )
    return x.save()
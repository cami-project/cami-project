from __future__ import unicode_literals

from django.db import models


# Create your models here.
class Notification(models.Model):
    class Meta:
        db_table = 'notification'

    notification_id = models.AutoField(primary_key=True)
    type = models.CharField(max_length=64)
    severity = models.CharField(max_length=32)
    message = models.CharField(max_length=1024)
    created = models.DateTimeField(auto_now_add=True)

from __future__ import unicode_literals

from django.db import models


class Notification(models.Model):
    class Meta:
        db_table = 'notification'

    NOTIFICATION_TYPES = (
        ('weight', 'weight'),
        ('heart', 'heart'),
        ('sleep', 'sleep'),
    )

    NOTIFICATION_STATUSES = (
        ('ok', 'ok'),
        ('warning', 'warning'),
        ('wakeup', 'wakeup'),
    )

    user_id = models.BigIntegerField(name='user_id')
    type = models.CharField(max_length=20, choices=NOTIFICATION_TYPES)
    status = models.CharField(max_length=20, choices=NOTIFICATION_STATUSES)
    timestamp = models.BigIntegerField()
    message = models.CharField(max_length=512)
    description = models.CharField(max_length=1024)
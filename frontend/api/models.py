from __future__ import unicode_literals

from django.db import models


class Notification(models.Model):
    class Meta:
        db_table = 'notification'

    RECIPIENT_TYPES = (
        ('elderly', 'elderly'),
        ('caregiver', 'caregiver')
    )

    NOTIFICATION_TYPES = (
        ('weight', 'weight'),
        ('heart', 'heart'),
        ('sleep', 'sleep')
    )

    NOTIFICATION_SEVERITIES = (
        ('low', 'low'),
        ('medium', 'medium'),
        ('high', 'high')
    )

    user_id = models.BigIntegerField(name='user_id')
    recipient_type = models.CharField(max_length=20, choices=RECIPIENT_TYPES)
    type = models.CharField(max_length=20, choices=NOTIFICATION_TYPES)
    severity = models.CharField(max_length=20, choices=NOTIFICATION_SEVERITIES)
    timestamp = models.BigIntegerField()
    message = models.CharField(max_length=512)
    description = models.CharField(max_length=1024)

class MobileNotificationKey(models.Model):
    class Meta:
        db_table = 'mobile_notification_key'

    MOBILE_OS = (
        ('ios', 'ios'),
        ('android', 'android')
    )

    RECIPIENT_TYPES = (
        ('elderly', 'elderly'),
        ('caregiver', 'caregiver')
    )

    user_id = models.BigIntegerField(name='user_id')
    recipient_type = models.CharField(max_length=20, choices=RECIPIENT_TYPES)
    mobile_os = models.CharField(max_length=20, choices=MOBILE_OS)
    mobile_key = models.CharField(max_length=1024)
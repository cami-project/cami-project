# -*- coding: utf-8 -*-
# Generated by Django 1.9.5 on 2017-08-02 18:06
from __future__ import unicode_literals

from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('store', '0006_pushnotificationdevice'),
    ]

    operations = [
        migrations.RemoveField(
            model_name='journalentry',
            name='measurement',
        ),
        migrations.AddField(
            model_name='journalentry',
            name='reference_id',
            field=models.IntegerField(blank=True, null=True),
        ),
        migrations.AlterField(
            model_name='journalentry',
            name='severity',
            field=models.CharField(choices=[(b'low', b'low'), (b'medium', b'medium'), (b'high', b'high'), (b'none', b'none')], max_length=20),
        ),
        migrations.AlterField(
            model_name='journalentry',
            name='type',
            field=models.CharField(choices=[(b'weight', b'weight'), (b'heart', b'heart'), (b'sleep', b'sleep'), (b'activity', b'activity')], max_length=20),
        ),
        migrations.AlterField(
            model_name='pushnotificationdevice',
            name='type',
            field=models.CharField(choices=[(b'APNS', b'Apple Push Notifications Service'), (b'GCM', b'Google Cloud Messaging'), (b'WNS', b'Windows Push Notification Services')], max_length=4),
        ),
    ]
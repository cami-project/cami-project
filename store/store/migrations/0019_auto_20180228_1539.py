# -*- coding: utf-8 -*-
# Generated by Django 1.9.5 on 2018-02-26 12:53
from __future__ import unicode_literals

from django.conf import settings
from django.db import migrations, models
import django.db.models.deletion
import django_mysql.models


class Migration(migrations.Migration):

    dependencies = [
        migrations.swappable_dependency(settings.AUTH_USER_MODEL),
        ('store', '0018_auto_20180226_1453'),
    ]

    operations = [
        migrations.AlterUniqueTogether(
            name='pushnotificationdevice',
            unique_together=set([('user', 'registration_id')]),
        ),
    ]

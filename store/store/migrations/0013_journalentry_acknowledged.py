# -*- coding: utf-8 -*-
# Generated by Django 1.9.5 on 2017-08-22 15:57
from __future__ import unicode_literals

from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('store', '0012_auto_20170822_1223'),
    ]

    operations = [
        migrations.AddField(
            model_name='journalentry',
            name='acknowledged',
            field=models.NullBooleanField(),
        ),
    ]

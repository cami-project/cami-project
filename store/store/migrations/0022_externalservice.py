# -*- coding: utf-8 -*-
# Generated by Django 1.9.5 on 2018-08-02 13:06
from __future__ import unicode_literals

from django.conf import settings
from django.db import migrations, models
import django.db.models.deletion
import django_mysql.models


class Migration(migrations.Migration):

    dependencies = [
        migrations.swappable_dependency(settings.AUTH_USER_MODEL),
        ('store', '0021_auto_20180309_1456'),
    ]

    operations = [
        migrations.CreateModel(
            name='ExternalService',
            fields=[
                ('id', models.AutoField(auto_created=True, primary_key=True, serialize=False, verbose_name='ID')),
                ('created_at', models.DateTimeField(auto_now_add=True)),
                ('updated_at', models.DateTimeField(auto_now=True)),
                ('name', models.CharField(max_length=32)),
                ('service_url', models.URLField(default=b'https://calendar.google.com')),
                ('access_info', django_mysql.models.JSONField(default=dict)),
                ('user', models.ForeignKey(on_delete=django.db.models.deletion.CASCADE, related_name='used_services', to=settings.AUTH_USER_MODEL)),
            ],
        ),
    ]
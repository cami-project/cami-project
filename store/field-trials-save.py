#!/usr/bin/env python
from email.mime.text import MIMEText
from email.mime.application import MIMEApplication
from email.mime.multipart import MIMEMultipart
from smtplib import SMTP
import smtplib
import sys

import argparse
import datetime
import getpass

import os, django
os.environ.setdefault("DJANGO_SETTINGS_MODULE", "store.settings")
django.setup()

from celery.utils.text import join
from store.models import User, Measurement, JournalEntry
from django.core import serializers

user_dict = {
    "ro" : 7,
    "pl" : 6,
    "dk" : 5,
    "dk_caregiver": 9,
    "pl_caregiver": 10,
    "ro_caregier": 8
}

def dump_data(user_abbrev, start_ts, end_ts):
    user_id = user_dict[user_abbrev]
    user = User.objects.get(id=user_id)
    measurements = Measurement.objects.filter(user=user, timestamp__gte=start_ts, timestamp__lte=end_ts)
    journal_entries = JournalEntry.objects.filter(user=user, timestamp__gte=start_ts, timestamp__lte=end_ts)

    json_meas_save = serializers.serialize('json', measurements)
    json_journal_save = serializers.serialize('json', journal_entries)

    filename_meas = "cami-trial-" + user_abbrev + "-" + "meas-dump-" + str(start_ts) + "-" + str(end_ts) + ".json"
    filename_journal = "cami-trial-" + user_abbrev + "-" + "journal-dump-" + str(start_ts) + "-" + str(end_ts) + ".json"

    with open(filename_meas, "w") as f:
        f.write(json_meas_save)

    with open(filename_journal, "w") as f:
        f.write(json_journal_save)

    send_email(user_abbrev, filename_meas, filename_journal)

def send_email(user, filename_meas, filename_journal):
    now = datetime.datetime.utcnow();

    recipients = ['alex.sorici@gmail.com']
    emaillist = [elem.strip().split(',') for elem in recipients]
    msg = MIMEMultipart()
    msg['Subject'] = "[CAMI Field Trials] Data dump for user " + str(user) + " on " + str(now)
    msg['From'] = 'proiect.cami@gmail.com'
    msg['Reply-to'] = 'proiect.cami@gmail.com'

    msg.preamble = 'Multipart massage.\n'

    part = MIMEText("Hi, please find the attached file")
    msg.attach(part)

    part = MIMEApplication(open(filename_meas, "rb").read())
    part.add_header('Content-Disposition', 'attachment', filename=filename_meas)
    msg.attach(part)

    part = MIMEApplication(open(filename_journal, "rb").read())
    part.add_header('Content-Disposition', 'attachment', filename=filename_journal)
    msg.attach(part)

    server = smtplib.SMTP('smtp.gmail.com:587')
    server.ehlo_or_helo_if_needed()
    server.starttls()
    server.ehlo_or_helo_if_needed()
    passwd = getpass.getpass()

    server.login("proiect.cami@gmail.com", passwd)

    server.sendmail(msg['From'], emaillist, msg.as_string())


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='Process some integers.')
    parser.add_argument('--user', dest='user', action='store', type=str,
                        choices=['ro', 'pl', 'dk', 'dk_caregiver', 'ro_caregiver', 'pl_caregiver'], required = True,
                        help='the two letter abbreviation for the user')
    parser.add_argument('--start_ts', dest='start_ts', action='store', type=int,
                        help='UNIX timestamp for the start of the data dump period')
    parser.add_argument('--end_ts', dest='end_ts', action='store', type=int,
                        help='UNIX timestamp for the end of the data dump period')

    args = parser.parse_args(sys.argv[1:])
    user = args.user
    start_ts = args.start_ts
    end_ts = args.end_ts

    print(user)
    print(start_ts)
    print(end_ts)

    dump_data(user, start_ts, end_ts)

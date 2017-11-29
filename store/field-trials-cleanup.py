#!/usr/bin/env python
import argparse
import datetime

import os, django
os.environ.setdefault("DJANGO_SETTINGS_MODULE", "store.settings")
django.setup()

from celery.utils.text import join
from store.models import User, Measurement, JournalEntry
from django.core import serializers

user_dict = {
    "ro" : 7,
    "pl" : 6,
    "dk" : 5
}

def clean_data(user_abbrev, start_ts, end_ts):
    user_id = user_dict[user_abbrev]
    user = User.objects.get(id=user_id)
    measurements = Measurement.objects.filter(user=user, timestamp__gte=start_ts, timestamp__lte=end_ts)
    journal_entries = JournalEntry.objects.filter(user=user, timestamp__gte=start_ts, timestamp__lte=end_ts)

    # remove data
    print("About to remove the following measurements ...")
    print(str(measurements))
    act = raw_input("Continue (y/n)?")
    if act == "y":
        measurements.delete()
    else:
        return

    print("About to remove the following journal entries ...")
    print(str(measurements))
    act = raw_input("Continue (y/n)?")
    if act == "y":
        journal_entries.delete()
    else:
        return


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='Process some integers.')
    parser.add_argument('--user', dest='user', action='store', type=str,
                        choices = ['ro', 'pl', 'dk'], required = True,
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

    clean_data(user, start_ts, end_ts)

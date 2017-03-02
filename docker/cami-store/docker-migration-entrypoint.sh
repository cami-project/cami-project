#!/usr/bin/env bash

# Using hardcoded values for paths for now.

python /cami-project/store/manage.py migrate
python /cami-project/store/manage.py loaddata initialize.yaml
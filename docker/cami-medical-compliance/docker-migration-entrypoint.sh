#!/usr/bin/env bash

# Using hardcoded values for paths for now.

python /cami-project/medical_compliance/manage.py migrate
python /cami-project/medical_compliance/manage.py loaddata initialize.yaml

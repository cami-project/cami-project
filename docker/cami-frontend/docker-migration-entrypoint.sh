#!/usr/bin/env bash

# Using hardcoded values for paths for now.
# TODO(@iprunache): find a way to make the paths configurable

python /cami-project/frontend/manage.py migrate
python /cami-project/frontend/manage.py loaddata initialize.yaml

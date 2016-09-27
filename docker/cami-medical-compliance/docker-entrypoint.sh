#!/usr/bin/env bash

# Using hardcoded values for paths for now.
# TODO(@iprunache): find a way to make the paths configurable

# Doing a migration at the container start is not a good idea on the long term
# as usually one wants containers to start as fast as possible.
python /cami-project/medical_compliance/manage.py runserver 0.0.0.0:8000

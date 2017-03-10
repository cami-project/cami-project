#!/usr/bin/env bash

# Export root of cami-project to PYTHONPATH, to make all services accessible as modules
export PYTHONPATH="${PYTHONPATH}:/cami-project"

# Using hardcoded values for paths for now.
# TODO(@iprunache): find a way to make the paths configurable

python /cami-project/frontend/manage.py runserver 0.0.0.0:8001

#!/usr/bin/env bash

# Export root of cami-project to PYTHONPATH, to make all services accessible as modules
export PYTHONPATH="${PYTHONPATH}:/cami-project"

# Start a celery worker for the frontend app.
cd /cami-project/frontend
celery -A frontend worker -l info

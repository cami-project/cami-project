#!/usr/bin/env bash

# Export root of cami-project to PYTHONPATH, to make all services accessible as modules
export PYTHONPATH="${PYTHONPATH}:/cami-project"

# Start a celery worker for the medical_compliance app.
cd /cami-project/linkwatch
celery -A tasks worker --loglevel=info

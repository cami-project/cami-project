#!/usr/bin/env bash

# Export root of cami-project to PYTHONPATH, to make all services accessible as modules
export PYTHONPATH="${PYTHONPATH}:/cami-project"

# Start a celery worker for the medical_compliance app.
cd /cami-project/medical_compliance
python run_celery_beat.py medical_compliance


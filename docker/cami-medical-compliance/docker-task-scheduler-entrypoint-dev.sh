#!/usr/bin/env bash

# Start a celery worker for the medical_compliance app.
cd /cami-project/medical_compliance
python run_celery_beat.py medical_compliance


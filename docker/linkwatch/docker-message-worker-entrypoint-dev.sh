#!/usr/bin/env bash

# Start a celery worker for the medical_compliance app.
cd /cami-project/linkwatch
python run_celery.py linkwatch

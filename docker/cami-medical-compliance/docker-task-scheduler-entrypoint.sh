#!/usr/bin/env bash

# Start a celery worker for the medical_compliance app.
cd /cami-project/medical_compliance
celery -A medical_compliance beat -l info

#!/usr/bin/env bash

# Remove busy pid
rm -f ./google_calendar/celerybeat.pid

# Export root of cami-project to PYTHONPATH, to make all services accessible as modules
export PYTHONPATH="${PYTHONPATH}:/cami-project"

# Start a celery worker for the Google Calendar app.
cd /cami-project/google_calendar
celery -A tasks beat -l info

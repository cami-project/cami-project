#!/usr/bin/env bash

# Start a celery worker for the frontend app.
cd /cami-project/frontend
celery -A frontend worker -l info

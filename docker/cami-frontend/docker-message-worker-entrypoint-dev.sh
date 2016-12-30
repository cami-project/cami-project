#!/usr/bin/env bash

# Start a celery worker for the frontend app.
cd /cami-project/frontend
python run_celery.py frontend

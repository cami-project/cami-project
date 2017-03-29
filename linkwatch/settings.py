# Celery settings
BROKER_URL = 'amqp://cami:cami@cami-rabbitmq:5672/cami'
BROKER_QUEUE = 'broadcast_measurement'
BROKER_TASK = 'cami.on_measurement_received'

LINKWATCH_URL_BASE = "https://linkwatchrestservicetest.azurewebsites.net/"
LINKWATCH_USER = 'CamiDemo'
LINKWATCH_PASSWORD = 'password'

PAPERTRAILS_LOGGING_HOSTNAME = 'logs4.papertrailapp.com'
PAPERTRAILS_LOGGING_PORT = 43843

try:
    from settings_local import *
except:
    pass

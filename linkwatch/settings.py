# Celery settings
BROKER_URL = 'amqp://cami:cami@cami-rabbitmq:5672/cami'
BROKER_QUEUE = 'broadcast_measurement'
BROKER_TASK = 'cami.parse_measurement'

LINKWATCH_URL_BASE = "https://linkwatchrestservicetest.azurewebsites.net/"
LINKWATCH_USER = 'CNetDemo'
LINKWATCH_PASSWORD = 'password'
# Celery settings
BROKER_URL = 'amqp://cami:cami@cami-rabbitmq:5672/cami'
BROKER_QUEUE = 'broadcast_measurement'
BROKER_TASK = 'cami.on_measurement_received'

OPENTELE_URL_BASE = "http://opentele.aliviate.dk:4388/opentele-citizen-server"
OPENTELE_USER = 'nancyann'
OPENTELE_PASSWORD = 'abcd1234'

PAPERTRAILS_LOGGING_HOSTNAME = 'logs4.papertrailapp.com'
PAPERTRAILS_LOGGING_PORT = 43843

try:
    from settings_local import *
except:
    pass

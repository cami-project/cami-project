from settings import *

DEBUG = True
DATABASES = {
    'default': {
        'ENGINE': 'django.db.backends.mysql',
        'NAME': 'cami',
        'USER': 'cami',
        'PASSWORD': 'cami',
        'HOST': '{INSERT THE MYSQL IP}',
        'PORT': '{INSERT THE MYSQL PORT}'
    }
}
BROKER_URL = 'amqp://cami:cami@{INSERT THE RABBITMQ IP}:{INSERT THE RABBITMQ AMPQ URL}/cami'
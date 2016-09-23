"""
Django settings for medical_compliance project.

Generated by 'django-admin startproject' using Django 1.9.5.

For more information on this file, see
https://docs.djangoproject.com/en/1.9/topics/settings/

For the full list of settings and their values, see
https://docs.djangoproject.com/en/1.9/ref/settings/
"""

import os

from kombu import Exchange, Queue

# Build paths inside the project like this: os.path.join(BASE_DIR, ...)
BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))


# Quick-start development settings - unsuitable for production
# See https://docs.djangoproject.com/en/1.9/howto/deployment/checklist/

# SECURITY WARNING: keep the secret key used in production secret!
SECRET_KEY = '(zawb(0eluv_yf-*r@_zpl--#3^5wgg2w)g83xd&1#59g)7&!b'

# SECURITY WARNING: don't run with debug turned on in production!
DEBUG = True

ALLOWED_HOSTS = []


# Application definition

INSTALLED_APPS = [
    'django.contrib.admin',
    'django.contrib.auth',
    'django.contrib.contenttypes',
    'django.contrib.sessions',
    'django.contrib.messages',
    'django.contrib.staticfiles',
    'corsheaders',
    'tastypie',
    'medical_compliance.api',
    'medical_compliance.api.analyzers'
]

MIDDLEWARE_CLASSES = [
    'django.middleware.security.SecurityMiddleware',
    'django.contrib.sessions.middleware.SessionMiddleware',
    'django.middleware.common.CommonMiddleware',
    'django.middleware.csrf.CsrfViewMiddleware',
    'django.contrib.auth.middleware.AuthenticationMiddleware',
    'django.contrib.auth.middleware.SessionAuthenticationMiddleware',
    'django.contrib.messages.middleware.MessageMiddleware',
    'django.middleware.clickjacking.XFrameOptionsMiddleware',
]

ROOT_URLCONF = 'medical_compliance.urls'

TEMPLATES = [
    {
        'BACKEND': 'django.template.backends.django.DjangoTemplates',
        'DIRS': [],
        'APP_DIRS': True,
        'OPTIONS': {
            'context_processors': [
                'django.template.context_processors.debug',
                'django.template.context_processors.request',
                'django.contrib.auth.context_processors.auth',
                'django.contrib.messages.context_processors.messages',
            ],
        },
    },
]

WSGI_APPLICATION = 'medical_compliance.wsgi.application'


# Database
# https://docs.djangoproject.com/en/1.9/ref/settings/#databases
DATABASES = {
    'default': {
        'ENGINE': 'django.db.backends.mysql',
        'NAME': 'cami',
        'USER': 'cami',
        'PASSWORD': 'cami',
        'HOST': 'cami-store',   #DEV : or actual db ip/hostname e.g. localhost for dev env
        'PORT': '3306' #DEV : or actual mysql port
    }
}


# Password validation
# https://docs.djangoproject.com/en/1.9/ref/settings/#auth-password-validators

AUTH_PASSWORD_VALIDATORS = [
    {
        'NAME': 'django.contrib.auth.password_validation.UserAttributeSimilarityValidator',
    },
    {
        'NAME': 'django.contrib.auth.password_validation.MinimumLengthValidator',
    },
    {
        'NAME': 'django.contrib.auth.password_validation.CommonPasswordValidator',
    },
    {
        'NAME': 'django.contrib.auth.password_validation.NumericPasswordValidator',
    },
]


# Internationalization
# https://docs.djangoproject.com/en/1.9/topics/i18n/

LANGUAGE_CODE = 'en-us'

TIME_ZONE = 'UTC'

USE_I18N = True

USE_L10N = True

USE_TZ = True

TASTYPIE_DEFAULT_FORMATS = ['json']

# Static files (CSS, JavaScript, Images)
# https://docs.djangoproject.com/en/1.9/howto/static-files/

STATIC_URL = '/static/'

CORS_ORIGIN_ALLOW_ALL = True
X_FRAME_OPTIONS='ALLOW-FROM *'


# WITHINGS API credentials
WITHINGS_USER_ID = 11262861
WITHINGS_CONSUMER_KEY = "734b6504c858bed3e3ecd7ce78b543f5f9e3cbe9b1b41fc40f012d1fdc96"
WITHINGS_CONSUMER_SECRET = "4f3d74e3f148781ee6459cc881186b24e2c3850530b6370e60d81b5822"

WITHINGS_OAUTH_V1_TOKEN = "394b9199422126c1ffc405bf003dd26cc6259cd02c8dee230b8bcd12634de5"
WITHINGS_OAUTH_V1_TOKEN_SECRET = "5643779f862c5030c5631fd3387233cbf8465b8520c5b4b073850fd"


# Celery settings
#DEV : replace 5672 with actual Local Port and cami-rabbitmq with the actual IP/hostname (e.g. amqp://cami:cami@127.0.0.1:32781/cami)
BROKER_URL = 'amqp://cami:cami@cami-rabbitmq:5672/cami'

CELERY_DEFAULT_QUEUE = 'withings_measurements'
CELERY_QUEUES = (
    Queue('withings_measurements', Exchange('withings_measurements'), routing_key='withings_measurements'),
    Queue('medical_compliance_measurements', Exchange('medical_compliance_measurements'), routing_key='medical_compliance_measurements'),
    Queue('medical_compliance_weight_analyzers', Exchange('medical_compliance_weight_analyzers'), routing_key='medical_compliance_weight_analyzers'),
)


LOGGING = {
    'version': 1,
    'disable_existing_loggers': False,
    'handlers': {
        'file': {
            'level': 'DEBUG',
            'class': 'logging.FileHandler',
            'filename': './debug.log',
        },
        'celery_file': {
            'level': 'DEBUG',
            'class': 'logging.FileHandler',
            'filename': './celery.log',
        }
    },
    'loggers': {
        'django': {
            'handlers': ['file'],
            'level': 'DEBUG',
            'propagate': True,
        },
        'medical_compliance.measurement_callback': {
            'handlers': ['file'],
            'level': 'DEBUG',
            'propagate': True,
        },
        'withings_controller.fetch_measurement': {
            'handlers': ['celery_file'],
            'level': 'DEBUG',
            'propagate': False,
        },
        'medical_compliance.fetch_weight_measurement': {
            'handlers': ['celery_file'],
            'level': 'DEBUG',
            'propagate': False,
        },
    },
}

try:
    from settings_local import *
except:
    pass

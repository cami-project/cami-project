import sys
import logging

from logging.handlers import SysLogHandler
from celery.utils.log import get_task_logger
from settings import PAPERTRAILS_LOGGING_HOSTNAME, PAPERTRAILS_LOGGING_PORT

logger = get_task_logger('linkwatch')
logger.setLevel(logging.DEBUG)

formatter = logging.Formatter('%(levelname)s %(asctime)s %(module)s '
                      '%(process)d %(thread)d %(message)s')

syslog = SysLogHandler(address=(PAPERTRAILS_LOGGING_HOSTNAME, PAPERTRAILS_LOGGING_PORT))
syslog.setFormatter(formatter)

logger.addHandler(syslog)

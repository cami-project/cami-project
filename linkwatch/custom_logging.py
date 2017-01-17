import logging
import settings

from logging.handlers import SysLogHandler

logger = logging.getLogger()
logger.setLevel(logging.DEBUG)

syslog = SysLogHandler(address=(settings.PAPERTRAILS_LOGGING_HOSTNAME, settings.PAPERTRAILS_LOGGING_PORT))
formatter = logging.Formatter('%(levelname)s %(asctime)s %(module)s '
                      '%(process)d %(thread)d %(message)s')

syslog.setFormatter(formatter)
logger.addHandler(syslog)

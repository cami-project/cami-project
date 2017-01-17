Application logs
=========================================
### Configure Papertrail logging
1. Django apps
In the application's ```settings.py``` file a new logging handler needs to be added:

```
'syslog': {
    'level':'DEBUG',
    'class':'logging.handlers.SysLogHandler',
    'formatter': 'verbose',
    'address':(PAPERTRAILS_LOGGING_HOSTNAME, PAPERTRAILS_LOGGING_PORT)
}
```
Add this new handler, ```syslog```, to all the loggers you would like to redirect also to Papertrail.

2. Plain Python apps
Alos the SysLogHandler is used to redirect logs to Papertrail.
The snippet below configures the Python logger to send logs to Papertrail:
```
import logging
from logging.handlers import SysLogHandler

logger = logging.getLogger()
logger.setLevel(logging.DEBUG)

syslog = SysLogHandler(address=(PAPERTRAILS_LOGGING_HOSTNAME, PAPERTRAILS_LOGGING_PORT))
formatter = logging.Formatter('%(levelname)s %(asctime)s %(module)s '
                      '%(process)d %(thread)d %(message)s')

syslog.setFormatter(formatter)
logger.addHandler(syslog)
```

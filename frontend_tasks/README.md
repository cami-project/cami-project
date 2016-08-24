Use this module to send tasks to the frontend service.

Usage:
```
from frontend_tasks import tasks

# Use delay to send the notification remotely.
tasks.send_notification.delay(
                              'Your blood pressure is high', 
                              'blood_presure',
                              'high')
```

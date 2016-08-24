import sys
import time
import requests

from frontend_tasks import tasks as frontend

if __name__ == '__main__':
    message = sys.argv[1]
    type = sys.argv[2]
    severity = sys.argv[3]

    # Send the notification to the frontend service
    #
    # What happens here is that the Celery worker running in the
    # cami-frontend-message-worker container will receive the task and save a
    # new notification in the DB using frontend's Notification model.
    frontend.send_notification.delay(message, type, severity)

    # Give it some time
    time.sleep(1)

    # Get the latest notifications
    r = requests.get(
        'http://127.0.0.1:8001/api/v1/notifications/',
        params={'limit': 5}
    )

    for n in r.json()['objects']:
        print('%s: %s (type: %s, severity %s)' % (
            n['created'],
            n['message'],
            n['type'],
            n['severity'],
        ))

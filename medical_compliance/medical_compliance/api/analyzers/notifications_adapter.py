import json

from kombu import Producer, Exchange, Connection

from django.conf import settings
from medical_compliance.api import store_utils


class NotificationsAdapter():
    def __send_push_notification(self, user_id, message):
        payload = {
            "user_id": user_id,
            "message": message
        }

        with Connection(settings.BROKER_URL) as conn:
            channel = conn.channel()

            inserter = Producer(
                exchange=Exchange('push_notifications', type='topic'),
                channel=channel,
                routing_key="push_notification"
            )
            inserter.publish(json.dumps(payload))

    def send_caregiver_notification(self, user_id, notification_type, severity, message, description, timestamp = None):
        self.__send_push_notification(3, message)

        store_utils.insert_journal_entry(
            user="/api/v1/user/%d/" % 3,
            type=notification_type,
            severity=severity,
            timestamp=timestamp,
            message=message,
            description=description,
            # This is dummy, will be removed anyway when refactoring medical_compliance
            measurement="/api/v1/measurement/%d/" % 1
        )

    def send_elderly_notification(self, user_id, notification_type, severity, message, description, timestamp = None):
        self.__send_push_notification(2, message)

        store_utils.insert_journal_entry(
            user="/api/v1/user/%d/" % 2,
            type=notification_type,
            severity=severity,
            timestamp=timestamp,
            message=message,
            description=description,
            # This is dummy, will be removed anyway when refactoring medical_compliance
            measurement="/api/v1/measurement/%d/" % 1
        )

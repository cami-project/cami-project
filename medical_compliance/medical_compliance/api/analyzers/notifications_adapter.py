import celery
from celery import Celery

from medical_compliance.api import store_utils

class NotificationsAdapter():
    __celery_notification_task = 'frontend.send_notification'
    __celery_queue = 'frontend_notifications'

    def __get_celery_app(self):
        app = Celery()
        app.config_from_object('django.conf:settings')
        return app

    def send_caregiver_notification(self, user_id, notification_type, severity, message, description, timestamp = None):
        celery_app = self.__get_celery_app()
        celery_app.send_task(
            self.__celery_notification_task,
            (
                3,
                message
            ),
            queue=self.__celery_queue
        )

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
        celery_app = self.__get_celery_app()
        celery_app.send_task(
            self.__celery_notification_task,
            (
                2,
                message
            ),
            queue=self.__celery_queue
        )

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

import celery
from celery import Celery

class NotificationsAdapter():
    __celery_notification_task = 'frontend.send_notification'
    __celery_queue = 'frontend_notifications'

    def __get_celery_app(self):
        app = Celery()
        app.config_from_object('django.conf:settings')
        return app

    def send_caregiver_notification(self, user_id, notification_type, severity, message, description):
        celery_app = self.__get_celery_app()
        celery_app.send_task(self.__celery_notification_task, (user_id, 'caregiver', notification_type, severity, message, description), queue=self.__celery_queue)

    def send_elderly_notification(self, user_id, notification_type, severity, message, description):
        celery_app = self.__get_celery_app()
        celery_app.send_task(self.__celery_notification_task, (user_id, 'elderly', notification_type, severity, message, description), queue=self.__celery_queue)
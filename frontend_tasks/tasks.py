from kombu import Queue, Exchange
from celery import Celery

app = Celery('api.tasks', broker='amqp://cami:cami@192.168.73.1:5673/cami')
app.conf.update(
    CELERY_DEFAULT_QUEUE='frontend',
    CELERY_QUEUES=(
        Queue('frontend', Exchange('frontend'), routing_key='frontend'),
    ),
)


@app.task(name='frontend.send_notification')
def send_notification(message, type, severity):
    pass

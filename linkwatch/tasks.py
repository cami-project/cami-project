from celery import Celery
from kombu import Queue, Exchange
from kombu.common import Broadcast
import endpoints

app = Celery('api.tasks', broker='amqp://cami:cami@127.0.0.1:5673/cami')
app.conf.update(
    CELERY_DEFAULT_QUEUE='broadcast_measurement',
    CELERY_QUEUES=(
        Broadcast('broadcast_measurement'),
    ),
)

@app.task(name='cami.parse_measurement')
def parse_measurement(measurement_json):
    endpoints.save_measurement(measurement_json)
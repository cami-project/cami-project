from __future__ import absolute_import

from . import settings
from kombu import Connection
import librato

USER_ENVIRONMENT = "user_environment"
USER_HEALTH = "user_health"
USER_NOTIFICATIONS = "user_notifications"

# When Django starts, we want to have already declared the exchanges for each type of measurement or event
with Connection(settings.BROKER_URL) as conn:
    channel = conn.channel()

    for exchange in settings.BROKER_EXCHANGES:
        # bind the exchange to the channel
        bound_exchange = exchange(channel)

        # declare the exchange
        bound_exchange.declare()

"""
Also, we want to setup the Librato metrics monitoring API.
So, upon startup, we will declare two monitoring queues, one for measurements and one for events, in the style
of the `consumer.py script`
"""
librato_api = librato.connect(settings.LIBRATO_EMAIL, settings.LIBRATO_TOKEN, sanitizer=librato.sanitize_metric_name)



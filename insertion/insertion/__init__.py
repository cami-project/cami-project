from __future__ import absolute_import

from . import settings
from kombu import Connection

# When Django starts, we want to have already declared the exchanges for each type of measurement or event
with Connection(settings.BROKER_URL) as conn:
    channel = conn.channel()

    for exchange in settings.BROKER_EXCHANGES:
        # bind the exchange to the channel
        bound_exchange = exchange(channel)

        # declare the exchange
        bound_exchange.declare()


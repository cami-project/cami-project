#!/usr/bin/env python
import pika

# These would need to be configurable when using the daemon as a real service.
credentials = pika.PlainCredentials('cami', 'cami')
parameters = pika.ConnectionParameters('192.168.73.13',
                                       5672,
                                       '/cami',
                                       credentials)
connection = pika.BlockingConnection(parameters)
channel = connection.channel()

# Create or check that the queue exists before trying to consume from it.
channel.queue_declare(queue='hello', durable=True)

def callback(ch, method, properties, body):
    print(" [x] Received %r" % body)

channel.basic_consume(callback,
                      queue='hello',
                      no_ack=True)

print(' [*] Waiting for messages. To exit press CTRL+C')
channel.start_consuming()

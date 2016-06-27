#!/usr/bin/env python
import pika

"""
Example of how to send a message through a RabbitMQ queue.
"""

credentials = pika.PlainCredentials('cami', 'cami')
parameters = pika.ConnectionParameters('192.168.73.13',
                                       5672,
                                       '/cami',
                                       credentials)

connection = pika.BlockingConnection(parameters)
channel = connection.channel()

# Create or check that the queue exists before sending the message.
channel.queue_declare(queue='hello', durable=True)

channel.basic_publish(exchange='',
                      routing_key='hello',
                      body='Hello World!')
print(" [x] Sent 'Hello World!'")
connection.close()

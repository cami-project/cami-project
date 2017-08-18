import pika

HOST_IP = "141.85.241.224"

parameters = pika.URLParameters('amqp://cami:cami@' + HOST_IP + ':5673/cami')
connection = pika.BlockingConnection(parameters)
channel = connection.channel()

# Create a temporary queue only for receiving data from the exchange
result = channel.queue_declare(exclusive=True)
queue_name = result.method.queue

channel.queue_bind(
	# We want to get 'measurement' data, so we bind with
	# the 'measurements' exchange
    exchange='measurements',
    queue=queue_name,
    # The 'measurements' exhange is of `topic` type
    # We can get the messages using wildcards
    # Using this routing key, we'll basically receive all
    # the measurements, no matter their type
    routing_key='measurement.*'
)

print(' [*] Waiting for logs. To exit press CTRL+C')

def callback(ch, method, properties, body):
    print(" [x] %r" % body)

channel.basic_consume(
    callback,
    queue=queue_name,
    no_ack=True
)

channel.start_consuming()
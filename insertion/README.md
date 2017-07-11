Data Insertion API container
============================

This container's purpose is to wrap an HTTP API over our internal RabbitMQ ecosystem. It's an input interface for CAMI System, through which one can insert data that will proceed through the full CAMI internal mechanism. For example, a (health) measurement can be inserted by an external source into our system, and that measurement will be sent to all the parts of the system that work with measurements, i.e. **Linkwatch** container and **OpenTele** container for sending it further to third parties, or **medical compliance** for analyzing the value, triggering an alert and finally inserting the value into the **Store**.


## Architecture

This is actually a Django application that sets up some HTTP endpoints for posting data by an established structure (see [CAMI Insertion API](cami-insertion-api.yml)). The data posted to the endpoints will be sent further to our internal [`RabbitMQ Exchanges`](https://lostechies.com/derekgreer/2012/03/28/rabbitmq-for-windows-exchange-types/). Then, that piece of data will get to any internal part of our system that has a queue listening on that particular Exchange and with the corresponding `routing key`.

The currently implemented Exchanges are:
* `measurements`: `routing_key = measurement.%%MEASUREMENT_TYPE%%`
* `events`: `routing_key = event.%%CATEGORY%%`
* `push_notifications`: `routing_key = push_notification`

All of the Exchanges are of `topic` type.

## Example
We have a simple example of a consumer and an external inserter. It can be found in the [`example`](example/) directory.

Steps for running the example:

0. Make sure the CAMI System is up and running

1. Install the requirements:

```
pip install -r requirements.txt
```

2. Open as many consumers as you want

```
python consumer.py
```

3. Send data using the HTTP Endpoints

This script will send a measurement to our insertion API, and the measurement will
show up in the consumers.

```
python insertion.py
```

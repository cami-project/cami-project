# cami-project
http://www.camiproject.eu/

# Setup using Docker

## Components

### cami-store
The MySQL image available on DockerHub uses MySQL's recommended configs for production which causes mysqld to eat up ~500MB when the container is started. Make sure that the system running the container has more than 1GB of RAM available.

Run the store container:
```
docker run -d --name cami-store -P cami/store:1.0
```

### cami-rabbitmq
First build the image(wihth credentials builtin).
```
docker build -t cami/rabbitmq:1.0 -f docker/cami-rabbitmq/Dockerfile .
```

Run a container. We need to specify the hostname since it si usde by rabbitmq
nodes to identify themselves.
```
docker run -d --hostname cami-rabbitmq --name cami-rabbitmq -P cami/rabbitmq:1.0
```

Get the port on which we can acces the rabbitmq management interface. In this
 case the default port was is
```
vagrant@docker:~/cami-project$ docker ps -l
CONTAINER ID        IMAGE               COMMAND                  CREATED             STATUS              PORTS                                                                                                                                                     NAMES
47101a9ced98        cami/rabbitmq:1.0   "docker-entrypoint.sh"   4 seconds ago       Up 2 seconds        0.0.0.0:32775->4369/tcp, 0.0.0.0:32774->5671/tcp, 0.0.0.0:32773->5672/tcp, 0.0.0.0:32772->15671/tcp, 0.0.0.0:32771->15672/tcp, 0.0.0.0:32770->25672/tcp   cami-rabbitmq
```

Access the management interface.
```
http://127.0.0.1:32771
```

### cami-medical-compliance
Build the image with docker:
```
docker build -t cami/medical-compliance:1.0 -f docker/cami-medical-compliance/Dockerfile .
```

To run the container you first need to have a running `cami-store` container for the mysql dependency:
`docker run -d --name cami-store -P cami/store:1.0`

Then run the `cami-medical-compliance` container linking it to the `cami-store` container.
``

# Setup using Vagrant

The project has Ansible receipts for setting up instances for all `cami` components inside
VirtualBox machines. Vagrant is used to manage the virtual machines.

Ansible is run as a local provisioner by Vagrant so it is not required on the host machine.

## Requirements

- VirtualBox
- Vagrant 1.8.1

## Setting up all instances
Setting up a working cluster only involves starting up the vagrant machines in a particular order.

```
cd cami-project/vagrant
vagrant up cami-store
vagrant up cami-rabbitmq-server
vagrant up cami-medical-compliance
```
You can check that all is in order by accessing this url: `http://192.168.73.12:8000/api/v1/medication-plans/`

Since the current setup does not populate the DB with data the answer should look like this:
```
{"meta": {"limit": 20, "next": null, "offset": 0, "previous": null, "total_count": 0}, "objects": []}
```

You can ssh to the vagrant machines from the vagrant folder:
```
cd cami-project/vagrant
vagrant ssh cami-store
```

## Components

### cami-store

This instance hosts the MySQL database that will be used as a local store by all **cami**
components.

The provisioning script installs a safe MySQL instance, creates a `cami` database and import a
basic schema for the database. It also creates a user with name `cami` and password `cami` that
has full privileges and can connect from any host.

The predefined ip for the `cami-store` instance is: `192.168.73.11`.

### cami-rabbitmq-server

This instance hosts a Rabbit MQ server that will be used as a message broker by all **cami**
components.

The provisioning script installs the RabbitMQ instance, creates a `cami` vhost and a `cami` user
with full permissions on that vhost.

Provisioning also enables the management plugin which allows managing the server through a web api.
The management interface can be accessed at `http://192.168.73.13:15672/` with user `guest` and
password `guest`.

The predefined ip for the `cami-rabbitmq-server` instance is: `192.168.73.13`.

### cami-medical-compliance

This instance hosts the medical compliance module that exposes a REST API through Tastypie over
Django. It connects to and uses the `cami` database on the `cami-store` instance.

The root of the REST API is: `http://192.168.73.12:8000/api/v1/`.

Example endpoints:
```
GET http://192.168.73.12:8000/api/v1/medication-plans/

{"meta": {"limit": 20, "next": null, "offset": 0, "previous": null, "total_count": 0}, "objects": []}

```

The predefined ip for the `cami-store` instance is: `192.168.73.12`.

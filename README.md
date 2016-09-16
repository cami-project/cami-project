# cami-project
http://www.camiproject.eu/

# Setup using Docker

The project now has a Docker containers configured for the microservices. This is still WIP but a functional dev env can be started using Docker.

You will need `docker-compose` to get the env running. Follow the [installation guide](https://docs.docker.com/compose/install/).

Once you have `docker-compose` installed just run this from the project root:
```
docker-compose up
```
This will start all cami microservices and output their standard output. It may take a while for all the containers to come online especially on the first run when the mysql database is initialised on cami-store. To check that all is working try this in your browser: `http://127.0.0.1:8000/api/v1/medication-plans/`. Replace `127.0.0.1` with your VM's ip if you're running Docker in a VM.

Check out frontend notification API at `http://127.0.0.1:8001/api/v1/notifications/?limit=2`.

You can also run the containers as daemons:
```
docker-compose up -d
```
Ouptut can be obtained with:
```
docker-compose logs
```

Use `docker-compose stop` to stop the containers or Ctrl+C to stop then when not running as daemon.

The docker-compose recipe is set up so that you can use the containers for development. All containers have the host project folder synced to the `/cami-project` folder from which the microservices are run. **Any change you do on the host will be reflected in the running apps.**

## Components

### cami-store
This instance hosts the MySQL database that will be used as a local store by all **cami** components.

The provisioning script installs a safe MySQL instance, creates a `cami` database and imports a basic schema for the database. It also creates a user with name `cami` and password `cami` that has full privileges and can connect from any host.

The MySQL image available on DockerHub uses MySQL's recommended configs for production which causes mysqld to eat up ~500MB when the container is started. Make sure that the system running the container has more than 1GB of RAM available.
``
Building the image
```
docker build -t cami/store:1.0 -f docker\cami-store\Dockerfile .
```
Run the store container:
```
docker run -d --hostname cami-store --name cami-store -P cami/store:1.0
```
If you're not running the database in a VM, then you need to obtain the local port that redirects to the docker container's `3306` (default mysql port). To obtain it run the command:
```
$ docker ps -l
```
And find the corresponding local port redirecting to 3306 from the output (e.g. 0.0.0.0:`32774`->3306) `[1]`

### cami-rabbitmq
This instance hosts a Rabbit MQ server that will be used as a message broker by all **cami** components. The provisioning script installs the RabbitMQ instance, creates a `cami` vhost and a `cami` user
with full permissions on that vhost.

Default ports for Rabbit MQ server are `15672` (for accessing the web console) amd `5672` for the amqp protocol used to send messages.

First build the image (the credentials are builtin).
```
docker build -t cami/rabbitmq:1.0 -f docker/cami-rabbitmq/Dockerfile .
```
Run a container. We need to specify the hostname since it is used by rabbitmq nodes to identify themselves.
```
docker run -d --hostname cami-rabbitmq --name cami-rabbitmq -P cami/rabbitmq:1.0
```

Get the corresponding local port on which we can acces the rabbitmq management interface and the amqp protocol. 
```
$ docker ps -l
```
In this case we search for the entries `0.0.0.0:32778->5672/tcp` and `0.0.0.0:32776->15672/tcp`, meaning that:
- to access the rabbitmq web admin console we use `http://{IP}:32776`
- the amqp rabbit url will be `amqp://cami:cami@{IP}:32776/cami`  `[2]`

### cami-medical-compliance
This instance hosts the medical compliance module that exposes a REST API through Tastypie over Django. It connects to and uses the `cami` database on the `cami-store` instance and also the RabbitMQ instance from cami-rabbitmq.

Build the image with docker:
```
docker build -t cami/medical-compliance:1.0 -f docker/cami-medical-compliance/Dockerfile .
```

To run the container you first need to have a running `cami-store` container for the mysql dependency and a running `cami-rabbitmq` container for the mq server (see prev 2 sections).

For the **development environment**, we need to extract the ports for the mysql-database and for the amqp url of rabbit mq: see `[1]` and `[2]`.
These should be placed in `cami-project/medical_compliance/medical_compliance/settings.py` where we see the `DEV` tags and **never be commited** in git.

For the next commands we need to be in the medical_compliance directory from the project.

We need to bootstrap the mysql database using the following command (should only be run `once`):
```
python medical_compliance/manage.py migrate
```

To run locally the medical_compliance [celery](http://www.celeryproject.org) tasks:
```
$ celery -A medical_compliance worker
```
This command should be left in a cmd and to add commands asynchronously we'll need a different terminal in the same path. To run add a task in the celery queue by name:
```
$ python manage.py shell
> import medical_compliance
> medical_compliance.celery.app.send_task('medical_compliance.fetch_measurement', [11262861, 1273406557553, 1473406557553, 1])
```

This app also features some REST api which can be open by running:
```
$ python manage.py runserver 0.0.0.0:8000
```
If you are using [Visual Studio Code](https://code.visualstudio.com/download), the previous command can also be invoked from the IDE by running the `medical_compliance` task which also supports attachments of breakpoints (these can be set directly from the editor).

Be sure to leave the celery worker always open as it needs to handle the async tasks.

# Setup using Vagrant (this is broken right now and might not get revived!)

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
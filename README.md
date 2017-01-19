# cami-project
http://www.camiproject.eu/

# Architecture
The system within this repository is made up of two main components:
* an iOS mobile application, based on React Native (Pepperoni boilerplate)
* a backend powered by microservices, deployed in DigitalOcean

For the mobile app, we have chosen React Native for its ease of use and for the possibility to easily port the application to Android, when the time comes.

For the backend, we have chosen a microservices-based infrastructure for several reasons:
* the consortium implementing CAMI is made up of multiple partners, who need to integrate their work
* microservices is the modern way of implementing multi-component architectures
* the underlying platform that we have chosen (Docker and Rancher) allow us to easily run the containers on any infrastructure, including your local development laptop

# Setup backend using Docker

The project now has a Docker containers configured for the microservices. You can either run them using the images saved on DockerHub (`Production`) or using the local code (`Development`).

You will need `docker-compose` to get the env running. Follow the [installation guide](https://docs.docker.com/compose/install/).

## Run for Production

To run the project using the images from DockerHub (`Production Only` - **this will not use the local code!**), execute the next command:

```
docker-compose -f docker-compose-prod.yml up
```

## Run for Development

If you want to open the development environment, you can use the following command from the project's root:
```
docker-compose up
```

This will start all cami microservices and output their standard output. It may take a while for all the containers to come online especially on the first run when the mysql database is initialised on cami-store. To check that all is working try this in your browser: `http://127.0.0.1:8000/api/v1/medication-plans/`. Replace `127.0.0.1` with your VM's ip if you're running Docker in a VM. Check out frontend notification API at `http://127.0.0.1:8001/api/v1/notifications/?limit=2`.

On each change of the code, the app will restart so you can test and develop the app fast. If the code does not reload, simply press Ctrl+C and run again the command.

The docker-compose recipe is set up so that you can use the containers for development. All the containers have the host project folder synced to the `/cami-project` folder from which the microservices are executed. **Any change you do on the host will be reflected in the running apps.**

You can also run the containers as daemons:
```
docker-compose up -d
```
Ouptut can be obtained with:
```
docker-compose logs
```

Use `docker-compose stop` to stop the containers or Ctrl+C to stop then when not running as daemon.

The next sections are optional and they can be used if you want to run all the components that you are developing separately outside Docker.

## Components

### cami-store
This instance hosts the MySQL database that will be used as a local store by all **cami** components.

The provisioning script installs a safe MySQL instance, creates a `cami` database and imports a basic schema for the database. It also creates a user with name `cami` and password `cami` that has full privileges and can connect from any host.

The MySQL image available on DockerHub uses MySQL's recommended configs for production which causes mysqld to eat up ~500MB when the container is started. Make sure that the system running the container has more than 1GB of RAM available.

Building the image
```
docker build -t cami/store:1.0 -f docker/cami-store/Dockerfile .
```
Run the store container:
```
docker run -d --hostname cami-store --name cami-store -P cami/store:1.0
```
You need to obtain the local port that redirects to the docker container's `3306` (default mysql port). To obtain it run the command:
```
$ docker ps -l
```
And find the corresponding local port redirecting to 3306 (default mysql port) from the output (e.g. 0.0.0.0:`32777`->3306) `[1]`. Note this as you will need it later.

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
In this case we search for the entries `0.0.0.0:32781->5672/tcp` (5672 is the default amqp port) and `0.0.0.0:32779->15672/tcp` (15672 is the default rabbitmq web console port), meaning that:
- to access the rabbitmq web admin console go to a browser and type `http://localhost:32779` (default credentials cami / cami)
- the amqp rabbit url will be `amqp://cami:cami@localhost:32781/cami`  `[2]`
* replace 32781 and 32779 with the actual local ports redirecting to 5672 and 15672

### cami-medical-compliance
This instance hosts the medical compliance module that exposes a REST API through Tastypie over Django. It connects to and uses the `cami` database on the `cami-store` instance and also the RabbitMQ instance from cami-rabbitmq.

Build the image with docker (**not needed for dev environment**):
```
docker build -t cami/medical-compliance:1.0 -f docker/cami-medical-compliance/Dockerfile .
```

To run the container you first need to have a running `cami-store` container for the mysql dependency and a running `cami-rabbitmq` container for the mq server (see prev 2 sections) (**not needed for dev environment**).
```
docker run -d --hostname cami-medical-compliance --name cami-medical-compliance -P cami/medical-compliance:1.0
```

For the next part we need to be in the `medical_compliance` directory from the project.

For the **development environment**, we need to extract the ports for the mysql-database and for the amqp url of rabbit mq: see `[1]` and `[2]`. Execute the next command to init the local settings file:

```
cp medical_compliance/settings_local_template.py medical_compliance/settings_local.py
```

These should be placed in `medical_compliance/settings_local.py`.

We need to bootstrap the mysql database using the following command (should only be run `once`) + install python dependencies:
```
pip install -r requirements.txt
python manage.py migrate --settings=medical_compliance.settings_local
```

To run locally the medical_compliance [celery](http://www.celeryproject.org) tasks:
```
$ celery -A medical_compliance worker
```
This command should be left in a cmd and to add tasks asynchronously we'll need a different terminal in the same path. To run add a task in the celery queue by name:
```
$ python manage.py shell
> import medical_compliance
> medical_compliance.celery.app.send_task('withings_controller.retrieve_and_save_withings_measurements', [11262861, 1273406557553, 1473406557553, 1])
```
The last command will generate some output in the celery task console (currently it has an error).

This app also features some REST api which can be open by running:
```
$ python manage.py runserver 0.0.0.0:8000 --settings=medical_compliance.settings_local
```
If you are using [Visual Studio Code](https://code.visualstudio.com/download), the previous command can also be invoked from the IDE by running the `medical_compliance` task which also supports attachments of breakpoints (these can be set directly from the editor).

Be sure to leave the celery worker always open as it needs to handle the async tasks.
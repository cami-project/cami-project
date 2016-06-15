# cami-project
http://www.camiproject.eu/

# Cluster setup

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

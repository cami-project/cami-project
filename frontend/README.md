Frontend container
==============
This is the container that handles direct communication with the end user applications (iOS App, Android App, Web App).

Currently, it's only used for sending push notifications to the iOS App.

## Push Notifications

There is a **RabbitMQ** `exchange` that is used inside the CAMI System for sending push notifications to the end users. This container is subscribed to that exchange through a queue, getting the push notifications requests from all the parts of the system and sending them to the users' devices.

The exchange definition: `Exchange('push_notifications', type='topic')`
The `routing_key` that it's used: `routing_key = push_notification`

Currently, only `Apple Push Notifications` are actually implemented, but the architecture also supports other push notifications providers, like `Google Cloud Messaging`.

The `Apple Push Notifications Service` implementation is originally taken from the [`django-push-notifications`](https://github.com/jleclanche/django-push-notifications/tree/master/push_notifications) project and modified to suit our project.

### Registering a push notification device into the system
There is an API endpoint for subscribing a user's device to the push notifications of that user:
**Endpoint**: `http://<cami_hostname_or_ip>:8001/api/v1/push-notification-subscribe`
**Method**: `POST`
**Payload Format**: `json`
**Payload**:
```
registration_id => the device token issued by the push notifications provider
service_type => the push notifications provider - one of (APNS, GCM, WNS)
user_id => the user's id from CAMI system
```
**Example**:
```
{
    "registration_id": "36b95015ffd6091576f9d68c0ad8a8ec5e4764acfd2a98d776db216f491eb794",
    "service_type": "APNS",
    "user_id": 2
}
```

### The payload of the push notifications queue messages
Sending push notifications over the **RabbitMQ** exchanges/queues is as simple as posting a `json` encoded string on the queue.

The messages that are posted on the queue must respect the following format:
```
user_id => the user's id from CAMI system
message => the actual, literal message to be sent to the user
```

**Example**
```
{
    "user_id": 2,
    "message": "Hey Jim! Your heart rate is quite low!"
}
```

### Generating iOS Push Notifications certificate

- To be able to send notifications using the APNS service, we need to configure the App ID in the Apple Dev console with development and production certificates
- In the `Certificates, Identifiers & Profiles` section, we need to edit the App Id used to bundle the app and enable Push Notifications; the steps from the Dev Console should be followed and in the end we will use the Mac OS Keychain app to export the private key + cert in the format required by Django
- Launch Keychain Access from your local Mac and from the login keychain, filter by the Certificates category. You will see an expandable option called `Apple Development Push Services`
- Right click on `Apple Development Push Services` > Export `Apple Development Push Services ID123`. Save this as `cert.p12` file somewhere you can access it. **Do no enter a password !!!**
- The next command generates the cert in Mac's Terminal for **PEM** format (Privacy Enhanced Mail Security Certificate):
    
`openssl pkcs12 -in cert.p12 -out apns-cert.pem -nodes -clcerts`

- The resulting **.pem** file needs to be copied in the `frontend/push-notifications/certificates/ios/prod/` or `frontend/push-notifications/certificates/ios/dev/` folder. The path is set in the Django settings file.
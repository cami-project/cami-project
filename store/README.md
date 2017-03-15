# CAMI Store API
This document presents the set of Resources defining the RESTful
API that allows interaction with the core CAMI Store service.

The set of currently implemented endpoints allows to:
  * get the list of CAMI Users (no profile included yet)
  * get the list of Devices
  * get the list of DeviceUsage (device-user pairs) elements, which
    specify which device is used by which user and what are the
    __credentials__ used to retrieve data from _that_ device, for _that_ user
  * filter DeviceUsage entries by device_id and different keys from the access_info JSON (which
    describes data access means: e.g. Withings API token, consumer key, user_id)
  * get the list of Measurements

## Endpoint details

#### List all Users:
```
http://<hostname_or_ip>:8000/api/v1/user/
```
<br/>

#### List all Devices:
```
http://<hostname_or_ip>:8000/api/v1/device/
```
<br/>

#### List all DeviceUsage entries:
```
http://<hostname_or_ip>:8000/api/v1/deviceusage/
```
<br/>

#### Filter DeviceUsage by device_id and access_info related elements:
```
http://<hostname_or_ip>:8000/api/v1/deviceusage/?device=1&access_info__withings_userid=11115034
```
The `device` filter takes the integer ID value of the device resource, i.e. from a DeviceResource
URI such as `/api/v1/device/1/`, the ID is the last integer number `1`.

Access information filtering is done as per indications of [JSONField key-presence lookups](http://django-mysql.readthedocs.io/en/latest/model_fields/json_field.html#key-presence-lookups).

In the example given, `access_info` is the name of the `JSONField` holding the access information
for the WS 30 scale from Withings and `withings_userid` is the __key__ in the JSON dict holding
the userid value for the Withings account.
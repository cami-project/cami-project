# CAMI Integrations

The CAMI project allows for all the stakeholders involved to contribute to the development and extension of its features. This entails that the information that is collected under the CAMI cloud must also be shared with the other stakeholders' systems.

> The purpose of this document is to have a centralized overview of the consortium's 3rd pary systems integrated with the CAMI Cloud.

## 1. Linkwatch

We're currently sharing the following information w/ Linkwatch via their API, as soon as we register the information inside CAMI Cloud:
* **weight measurements**
* **heart rate measurements**
* **steps count measurements**

#### API Documentation
* Look over the [API's endpoints list](https://linkwatchrestservicetest.azurewebsites.net/Help/)
* Play around with the API using [Postman](https://www.getpostman.com/) by importing [the compiled list](https://www.getpostman.com/collections/3610b13f2b4f37f0223e) of Postman API interactions for the Linkwatch API

#### Testing
* Use the information provided inside the [TESTING](TESTING.md) document to populate the CAMI Cloud with new measurements
* Vist [Linkwatch's website](http://www.mylinkwatch.se/) and login using the credentials from LastPass
* If all went well, the updated measurements should be visible on the [My Measurements page](http://www.mylinkwatch.se/my-measurements/)


## 2. OpenTele

We're currently sharing the following information w/ OpenTele via their API, as soon as we register the information inside CAMI Cloud:
* **weight measurements**
* **heart rate measurements**

#### API Documentation
* Look over the [API's endpoints list](https://bitbucket.org/4s/opentele2-citizen-server/src/afa35a584cbe2efe5adbdcc3dcd152d294243a34/docs/patient-api/PatientApi.md?at=master&fileviewer=file-view-default)
* Play around with the API using [Postman](https://www.getpostman.com/) by importing [the compiled list](https://www.getpostman.com/collections/c03e6655dcb424673961) of Postman API interactions for the Linkwatch API
  * You will also need to import the [environment configuration file](https://drive.google.com/open?id=0B5R_gDLmaW5Udk8zQVVPVVhfTnc) in order to have it working

#### Testing
* Use the information provided inside the [TESTING](TESTING.md) document to populate the CAMI Cloud with new measurements
* Vist [OpenTele's dashboard](https://opentele.aliviate.dk:4387/opentele-server/) and login using the credentials from LastPass
* If all went well, the updated measurements should be visible on the [CAMI measurements page](https://opentele.aliviate.dk:4387/opentele-server/patient/questionnaires/13)
  * Take in consideration that the interface isn't translated to English yet

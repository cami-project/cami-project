# CAMI Integrations

The CAMI project allows for all the stakeholders involved to contribute to the development and extension of its features. This entails that the information that is collected under the CAMI cloud must also be shared with the other stakeholders' systems.

> The purpose of this document is to have a centralized overview of the consortium's 3rd pary systems integrated with the CAMI Cloud.

## 1. Linkwatch

We're currently sharing the following information w/ Linkwatch via their API, as soon as we register the information inside CAMI Cloud:
* weight measurements
* heart rate measurements
* steps count measurements

#### API Documentation
* Look over the [API's endpoints list](https://linkwatchrestservicetest.azurewebsites.net/Help/)
* Play around with the API using [Postman](https://www.getpostman.com/)
  * Import [the compiled list](https://www.getpostman.com/collections/3610b13f2b4f37f0223e) of Postman API interactions for the Linkwatch API

#### Testing
* Use the information provided inside the [TESTING](TESTING.md) document to populate the CAMI Cloud with new measurements
* Vist [Linkwatch's website](http://www.mylinkwatch.se/) and login using the credentials from LastPass
* If all went well, the updated measurements should be visible on the [My Measurements page](http://www.mylinkwatch.se/my-measurements/)
# Testing CAMI Cloud's Functionality

> The purpose of this document is to centralize the information required for insuring that CAMI Cloud functions properly.

## Testing weight measurements

CAMI Cloud integrates with the [Withings](https://www.withings.com/eu/en/) API for gathering weight measurements. Here's what's needed to test if measurements are processed accordingly throughout the CAMI Cloud system:

* Visit [Withings](https://www.withings.com/eu/en/) and login using the credentials stored in LastPass
* From the [Withings Health Mate](https://healthmate.withings.com/) Dashboard, press the **Add a measurement button** and add an arbitraruy new weight from the resulting modal window
* The newly entered value will now be fetched and processed by the CAMI Cloud and show up on the various systems connected to it, like:
  * the Papertrail logging system - credentials to access it are stored inside LastPass
  * the CAMI iOS application
  * the Linkwatch website - find out more in the [Integrations document](INTEGRATIONS.md)
  * the OpenTele website - find out more in the [Integrations document](INTEGRATIONS.md)
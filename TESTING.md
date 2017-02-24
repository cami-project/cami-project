# Testing CAMI Cloud's Functionality

The purpose of this document is to centralize the information required for insuring that CAMI Cloud functions properly.

## Testing weight measurements

CAMI Cloud integrates with the [Withings](https://www.withings.com/eu/en/) API for gathering weight measurements. Here's what's needed to test if measurements are processed accordingly throughout the CAMI Cloud system:

* Visit [Withings](https://www.withings.com/eu/en/) and login using the credentials stored in LastPass
* From the [Withings Health Mate](https://healthmate.withings.com/) Dashboard, press the **Add a measurement button** and add an arbitraruy new weight from the resulting modal window
* The newly entered value will now be fetched and processed by the CAMI Cloud and show up on the various systems connected to it, like:
  * the Papertrail logging system - credentials to access it are stored inside LastPass
  * the CAMI iOS application
  * the Linkwatch website - find out more in the [Integrations](INTEGRATIONS.md) document
  * the OpenTele dashboard - find out more in the [Integrations](INTEGRATIONS.md) document

## Testing heart rate & steps measurements

CAMI Cloud integrates with the [Google Fit](https://developers.google.com/fit/) API for gathering steps & hear rate measurements. Here's what's needed to test if measurements are processed accordingly throughout the CAMI Cloud system:

The following scripts will need to be run inside the `google_fit/` directory.

### Setup

1. Download `config.py` from the CAMI folder on VS Google Drive and put it inside google_fit/client folder.
2. Run `pip install -r requirements.txt` to install the required python libraries.
3. Run `python get_refresh_token.py`
4. Copy the link you receive and open in inside an **incognito browser window**
5. Login with the test account you'll find inside LastPass
6. You'll then be prompted to grant some permissions so go ahead and do it
7. Although you'll be redirected to http://google.ro, **the redirect url contains useful information**; copy the url from the address bar and paste it in the script's console
8. The script will automatically write the resulting token inside the `refresh_token` file inside the `google_fit/` directory.

> Now you're ready to run the test scripts.

### Read data from Google Fit

Just run `get_test_hr_data.py` or `get_test_steps_data.py` scripts. The scripts will print data both from mock and real data streams.

###### Heart Rate
* run `python get_test_hr_data.py` -- heart rate data inside the terminal
* run `python get_test_hr_data.py > heartrate.txt` -- heart rate data inside a txt file

###### Steps Count
* run `python get_test_steps_data.py` -- steps data inside the terminal
* run `python get_test_steps_data.py > steps.txt` -- steps data inside a txt file


### Write data to Google Fit

To ease the testing process CAMI cloud is set to fetch the values from these test datastream as well.

###### Heart Rate

The script will write the value in ```CAMI Heart Rate Test``` datastream.

* run `python add_test_hr_value.py HEART_RATE_VALUE`

###### Steps Count

The script will write the value in ```CAMI Steps Test``` datastream.

* run `python add_test_steps_value.py NUMBER_OF_STEPS`

### Checking if values have been updated

The newly entered value will now be fetched and processed by the CAMI Cloud and show up on the various systems connected to it, like:

* the Papertrail logging system - credentials to access it are stored inside LastPass
* the CAMI iOS application
* the Linkwatch website - find out more in the [Integrations](INTEGRATIONS.md) document
* the OpenTele dashboard - find out more in the [Integrations](INTEGRATIONS.md) document


### GoogleFit Step Measurements Additional Information

There are multiple data sources in Google Fit that could be queried in order to retrieve step measurements:

###### Google

```
datastream_name: merge_step_deltas
datastream_id: derived:com.google.step_count.delta:com.google.android.gms:merge_step_deltas - this one aggregates step measurements from
```

###### LG Watch

```
datastream_name: derive_step_deltas<-raw:com.google.step_count.cumulative:LGE:LG Watch Urbane:f0a4073e:Step Counter
datastream_id: derived:com.google.step_count.delta:com.google.android.gms:LGE:LG Watch Urbane:f0a4073e:derive_step_deltas<-raw:com.google.step_count.cumulative:LGE:LG Watch Urbane:f0a4073e:Step Counter - this one returns the measurements taken by the LG Watch
```

> `f0a4073e` will be different for each user

We will be using the dedicated LG Watch datasource for the moment.


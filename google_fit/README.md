How to run the scripts
======================
### Step 0 - Download ```config.py``` from CAMI folder from VS Google Drive and put it in google_fit/client folder.
### Step 1 - Run ```pip install -r requirements.txt``` to install the required python libraries
### Step 2 - Go to login page to authorize our application and get a refresh token
1. Run get_refresh_token.py

 ```
 python get_refresh_token.py
 ```
2. Ctrl + left click on the link you get from the script
3. Login with the test account you'll find on drive
4. You'll be prompted to grant some permissions, do it
5. You'll be redirected to http://google.ro, but the redirect url contains useful information; copy the url from the address bar and paste it in the script's console
6. The script will write the refresh_token in a file

Now you're ready to run ```get_hr_data.py```, ```get_steps_data.py```, ```add_test_steps_value.py``` or ```add_test_hr_value.py```.

### Read data from Google Fit
Just run ```get_hr_data.py``` or ```get_steps_data.py``` scripts. The scripts will print data both from mock and real data streams.

```
python get_hr_data.py
```
or 
```
python get_hr_data.py > heartrate
```
to save the response in ```heartrate``` file

```
python get_steps_data.py
```
or 
```
python get_steps_data.py > steps
```
to save the response in ```steps``` file

### Write data to Google Fit
Run
```
python add_test_hr_value.py HEART_RATE_VALUE
```
The script will write the value in ```CAMI Heart Rate Test``` datastream. CAMI cloud it's set to fetch the values from this datastream also, for easying the testing process.

```
python add_test_steps_value.py NUMBER_OF_STEPS
```
The script will write the value in ```CAMI Steps Test``` datastream. CAMI cloud it's set to fetch the values from this datastream also, tsfor easying the testing process.

### GoogleFit step measurements

There are multiple data sources in Google Fit that could be queried in order to retrieve step measurements:

Google:
    datastream_name: merge_step_deltas
    datastream_id: derived:com.google.step_count.delta:com.google.android.gms:merge_step_deltas - this one aggregates step measurements from 

LG Watch:
    datastream_name: derive_step_deltas<-raw:com.google.step_count.cumulative:LGE:LG Watch Urbane:f0a4073e:Step Counter
    datastream_id: derived:com.google.step_count.delta:com.google.android.gms:LGE:LG Watch Urbane:f0a4073e:derive_step_deltas<-raw:com.google.step_count.cumulative:LGE:LG Watch Urbane:f0a4073e:Step Counter - this one returns the measurements taken by the LG Watch

    'f0a4073e' will be different for each user

We will use the dedicated LG Watch datasource for the moment.

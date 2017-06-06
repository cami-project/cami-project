Setting the LG Watch with the CAMI system
=========================================
### 1. Pair the watch with your Android phone
In order to do this, you need to have Android 4.3 or up.
Follow the steps:

 1. Reset the watch to the factory settings
 2. Install [Android Wear](https://play.google.com/store/apps/details?id=com.google.android.wearable.app&hl=en) app
 3. Turn on your Bluetooth
 4. Open the app and follow the onscreen instructions

### 2. Install and configure Cinch app
1. Install [Cinch](https://play.google.com/store/apps/details?id=com.ryansteckler.perfectcinch&hl=en)
2. Connect the app with your Google account and give the permission for Google Fit
3. Go to settings
4. Set ```Automatic update frequency``` to 5 minutes
5. Uncheck ```Remind me to keep moving```

By now, your phone should be able to automatically take your Heart Rate every 5 minutes, so ensure everything works as intended before proceeding with the next step:

 1. Ensure the Bluetooth is on
 2. Ensure the watch is connected with the phone
 3. Ensure Cinch is properly sending measurement requests to the watch

### 3. Get a Google Fit access token and write it in the project settings
#### A. Get the access token
0. Go to ```cami_project/google_fit``` directory
1. Download ```config.py``` from CAMI folder from VS Google Drive and put it in the current directory
2. Run ```pip install -r requirements.txt``` to install the required python libraries
3. Run get_refresh_token.py

 ```
 python get_refresh_token.py
 ```
4. Ctrl + left click on the link you get from the script
5. Login with the same Google account that you use on the Android phone
6. You'll be prompted to grant some permissions, do it
7. You'll be redirected to http://google.ro, but the redirect url contains useful information; copy the url from the address bar and paste it in the script's console
8. The script will write the token in ```refresh_token``` file
9. Get the token from the file and proceed to Step B

#### B. Write the token in the settings file
1. Go to ```cami_project/medical_compliance/medical_compliance```
2. Open ```settings.py``` file
3. Write the token in the ```GOOGLE_FIT_REFRESH_TOKEN``` var
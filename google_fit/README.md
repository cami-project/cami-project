How to run the scripts
======================
### Step 0 - Download ```config.py``` from CAMI folder from VS Google Drive and put it in the same folder with the scripts
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

Now you're ready to run ```get_data.py``` or ```write_data.py```.

### Read data from Google Fit
Just run get_data.py script. The script will print only the data from Cinch datastream to stdout by default.

```
python get_data.py
```
or 
```
python get_data.py > heartrate
```
if you want to get the result in ```heartrate``` file.

### Write data to Google Fit
Run
```
python write_data.py HEART_RATE_VALUE
```
The script will write the value in ```CAMI Heart Rate Test``` datastream. CAMI cloud it's set to fetch the values from this datastream also, for easying the testing process.
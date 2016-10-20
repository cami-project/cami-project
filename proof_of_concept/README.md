How to run the script
=====================

### Step 1 - Go to login page to authorize our application and get a refresh token
1. Run get_refresh_token.py
 ```
 python get_refresh_token.py
 ```
2. Ctrl + left click on the link you get from the script
3. Login with the test account you'll find on drive
4. You'll be prompted to grant some permissions, do it
5. You'll be redirected to http://google.ro, but the redirect url contains useful information; copy the url from the address bar and paste it in the script's console
6. The script will write the refresh_token in a file

### Step 2 - Authenticate and read data from Google Fit
Just run get_data.py script. The script will print the data to stdout by default.

```
python get_data.py
```
or 
```
python get_data.py > heartrate
```
if you want to get the result in ```heartrate``` file.
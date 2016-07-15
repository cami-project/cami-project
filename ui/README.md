Start the server with `./mock_api_server`. It starts a SimpleHTTPServer on port 8000 that serves the local dir.

Make sure that `env.js` in the client app root exists and points the UI REST API to the correct host an port. Then start the application:
```
cd cami-project/application
react-native run-ios
```

Push predefined notifications with `./pus_low_notification`, `./push_medium_notification`, `./push_high_notification`.

Push custom notifications with `./push_notification [type] [severity] [message]`. For example:
```
./push_notification blood_pressure high 'Your blood pressure is runnig high. Make sure you get some rest!`
```

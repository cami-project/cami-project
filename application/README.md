# Steps to run the iOS App locally
- Clone the project
- Go to the `application` folder
- Run the following commands
```
cp env.example.js env.js
npm install
```

`Note`: This will use the Rest API of the backend deployed on Digital Ocean. If you want to link the mobile app with the local REST Api endpoint, just go to the env.js file and replace all the IPs with `127.0.0.1`.

- Go to the ios folder and open the project with **XCode 8** by double clicking on `Cami.xcworkspace`
- From the Project Navigator, expand Cami > Libraries and click on `RCTWebSocket.xcodeproj`
- Go to Build Settings, find the **Other Warning Flags** and remove the entries `-Wall` and `-Werror`
- Expand Cami > Cami and open the file `ABNumberFormatter` 
- Change line 72 from 
> **override func stringForObjectValue(obj: AnyObject?) -> String?** 

to
> **override func stringForObjectValue(obj: AnyObject`?`) -> String?** 

- Clean (Shift+Cmd+K), Build (Cmd+B) and run the project from XCode by selecting a target (e.g. Simulator)


## Notes
For the moment the live refresh is not working, so before running the project, be sure to delete the old one from the device/simulator.

If you have trouble building the app because of missing `Lock.h` file make sure to take a look at https://www.npmjs.com/package/react-native-lock#cocoapods-with-uses_framework-flag.
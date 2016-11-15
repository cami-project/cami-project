# Steps to generate the iOS Push notifications certificate

- To be able to send notifications using the APNS service, we need to configure the App ID in the Apple Dev console with development and production certificates
- In the `Certificates, Identifiers & Profiles` section, we need to edit the App Id used to bundle the app and enable Push Notifications; the steps from the Dev Console should be followed and in the end we will use the Mac OS Keychain app to export the private key + cert in the format required by Django
- Launch Keychain Access from your local Mac and from the login keychain, filter by the Certificates category. You will see an expandable option called “Apple Development Push Services”
- Right click on “Apple Development Push Services” > Export “Apple Development Push Services ID123″. Save this as cert.p12 file somewhere you can access it. **Do no enter a password !!!**
- The next command generates the cert in Mac’s Terminal for PEM format (Privacy Enhanced Mail Security Certificate):
    
    `openssl pkcs12 -in cert.p12 -out apns-cert.pem -nodes -clcerts`

- The resulting .pem file needs to be copied in the `frontend/push-notifications/ios/prod/` or frontend/push-notifications/ios/dev/ folder. The path is set in the Django settings file.


# Resend the last notifications
- the frontend project features a REST endpoint that will resend the notification when accessed (through a GET request - so opened from any browser).
- on Rancher we can use [this URL](http://cami.vitaminsoftware.com:8001/api/v1/mobile-notification-key/resend/)
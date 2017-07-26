import * as env from '../../env';
import Auth0Lock from 'react-native-lock';
import * as AuthStateActions from '../modules/auth/AuthState';
import store from '../redux/store';
const {Platform} = require('react-native');
import Promise from 'bluebird';
import * as HomepageStateActions from '../modules/homepage/HomepageState'
import * as HomepageCaregiverStateActions from '../modules/homepage-caregiver/HomepageState'

import Color from 'color';
import variables from '../modules/variables/ElderGlobalVariables';

import { redirectToElderlyPage, redirectToOnboardingPage } from '../modules/navigation/NavigationState';

const clientId = env.AUTH0_CLIENT_ID;
const domain = env.AUTH0_DOMAIN;
const authenticationEnabled = clientId && domain;
const notificationsSubscriptionApi = env.NOTIFICATIONS_SUBSCRIPTION_API;
const mobileNotificationKeyApi = env.MOBILE_NOTIFICATION_KEY_API;

let lock = null;
if (authenticationEnabled) {
    lock = new Auth0Lock({
        clientId,
        domain
    });
} else {
    console.warn('Authentication not enabled: Auth0 configuration not provided');
}

export function showLogin() {
    if (!authenticationEnabled) {
        return;
    }

    const options = {
        closable: false,
        disableSignUp: true,
        connections: ["cami"]
    };

    if (Platform.OS === 'ios') {
        lock.customizeTheme({
            A0ThemePrimaryButtonNormalColor: variables.colors.status.low,
            A0ThemePrimaryButtonHighlightedColor: Color(variables.colors.status.low).darken(.25).hexString(),
            A0ThemeSecondaryButtonTextColor: variables.colors.gray.dark,
            A0ThemeTextFieldTextColor: variables.colors.gray.dark,
            A0ThemeTextFieldPlaceholderTextColor: variables.colors.gray.neutral,
            A0ThemeTextFieldIconColor: variables.colors.status.low,
            A0ThemeTitleTextColor: variables.colors.gray.dark,
            A0ThemeDescriptionTextColor: variables.colors.gray.dark,
            A0ThemeSeparatorTextColor: variables.colors.gray.light,
            A0ThemeScreenBackgroundColor: variables.colors.gray.lightest,
            A0ThemeIconBackgroundColor: variables.colors.gray.lightest,
            A0ThemeIconImageName: '',
            A0ThemeCredentialBoxBorderColor: '' //transparent
        });
    }

    lock.show(options, (err, profile, token) => {
        if (err) {
            store.dispatch(AuthStateActions.onUserLoginError(err));
            return;
        }

        // Authentication worked!
        // -- time to trigger data fetch for the specific user
        store.dispatch(AuthStateActions.onUserLoginSuccess(profile, token)).then(() => {
            var userType = profile.userMetadata.userType;

            console.log('[auth] - user logged in, figuring out data fetch & interface redirect');

            if (userType == 'elderly') {
                console.log('[auth] - user is [elder]. fetching data before redirecting...');

                var promise_elder = new Promise((resolve, reject) => {
                    store.dispatch(HomepageStateActions.requestNotification());
                    resolve("success");
                });

                promise_elder.then(() => {
                    console.log('[auth] - data has been fetched for [elder]. redirecting to homescreen');

                    store.dispatch(redirectToElderlyPage());

                    console.log('[auth] - successfully, redirected [elder] to the homescreen')
                })

            } else {
                console.log('[auth] - user is [caregiver]. fetching data before redirecting...');

                var promise_caregiver = new Promise((resolve, reject) => {
                    store.dispatch(HomepageCaregiverStateActions.requestCaregiverData());
                    resolve("success");
                });

                promise_caregiver.then(() => {
                    console.log('[auth] - data has been fetched for [caregiver]. redirecting to homescreen...');

                    store.dispatch(redirectToOnboardingPage());

                    console.log('[auth] - successfully, redirected [caregiver] to the homescreen')
                });
            }

            console.log('[auth] - subscribing to withings & push notifications...');

            // TODO @rtud: related to Withings subscribing in Medical Compliance container
            // - will need tending to after we get to the MC refactoring
            fetch(notificationsSubscriptionApi).then((response) => {
                console.log('[auth] - withings subscribing successful:');
                console.log(response);
            }).catch((error) => {
                console.log('[auth] - failed to subscribe to withings:');
                console.warning(error);
            });

            const pushNotificationsState = store.getState().get('pushNotifications');
            var didReceiveKey = pushNotificationsState.getIn(['didReceiveKey']);
            if (didReceiveKey) {
                mobileNotificationKey = pushNotificationsState.getIn(['mobileNotificationKey']);
                mobileOS = pushNotificationsState.getIn(['mobileOS']);

                console.log('[auth] - submitting push notifications information...');

                fetch(mobileNotificationKeyApi, {
                    method: "POST",
                    headers: {
                        'Accept': 'application/json',
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        registration_id: mobileNotificationKey,
                        user_id: profile.userMetadata.user_id,
                        service_type: "APNS",
                    })
                }).then((response) => {
                    console.log('[auth] - push notifications infotmation submitted succesfully:');
                    console.log(response);
                }).catch((error) => {
                    console.log('[auth] - failed to submit push notifications information:');
                    console.warning(error);
                });
            }
        })
    })
}

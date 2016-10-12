import * as env from '../../env';
import Auth0Lock from 'react-native-lock';
import * as AuthStateActions from '../modules/auth/AuthState';
import store from '../redux/store';
const {Platform} = require('react-native');

import Color from 'color';
import variables from '../modules/variables/ElderGlobalVariables';

import {redirectToHomePage} from '../modules/navigation/NavigationState';

const clientId = env.AUTH0_CLIENT_ID;
const domain = env.AUTH0_DOMAIN;
const authenticationEnabled = clientId && domain;
const notificationsSubscriptionApi = env.NOTIFICATIONS_SUBSCRIPTION_API;

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
    store.dispatch(AuthStateActions.onUserLoginSuccess(profile, token));
    store.dispatch(redirectToHomePage(profile.userMetadata.userType));

    fetch(notificationsSubscriptionApi).then((response) => {
    }).catch((error) => {
    });
  });
}

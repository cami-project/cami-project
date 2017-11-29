import React, {PropTypes} from 'react';
import {View, StyleSheet, ActivityIndicator, Alert, AppState} from 'react-native';
import NavigationViewContainer from './navigation/NavigationViewContainer';
import AppRouter from './AppRouter';
import * as auth0 from '../services/auth0';
import * as snapshotUtil from '../utils/snapshot';
import * as SessionStateActions from '../modules/session/SessionState';
import store from '../redux/store';
import DeveloperMenu from '../components/DeveloperMenu';
import PushNotification from 'react-native-push-notification';
import * as PushNotificationsState from './push-notifications/PushNotificationsState';
import * as HomepageStateActions from './homepage/HomepageState';

const AppView = React.createClass({
  propTypes: {
    isReady: PropTypes.bool.isRequired,
    isLoggedIn: PropTypes.bool.isRequired,
    dispatch: PropTypes.func.isRequired
  },
  componentDidMount() {
    snapshotUtil.resetSnapshot()
      .then(snapshot => {
        const {dispatch} = this.props;

        if (snapshot) {
          dispatch(SessionStateActions.resetSessionStateFromSnapshot(snapshot));
        } else {
          dispatch(SessionStateActions.initializeSessionState());
        }

        store.subscribe(() => {
          snapshotUtil.saveSnapshot(store.getState());
        });
      });

      PushNotification.configure({
        onRegister: ((token) => {
          this.props.dispatch(PushNotificationsState.didReceiveMobileNotificationKey(token.token, token.os));
        }),
        onNotification: ((notification) => {
          Alert.alert('New Notification', notification.message);
        }),
        popInitialNotification: true,
        requestPermissions: true
      });
  },

  componentWillReceiveProps({isReady, isLoggedIn}) {
    console.log('[AppView] - this.props.isReady: ' + this.props.isReady + ', isReady: ' + isReady + ', this.props.isLoggedIn: ' + this.props.isLoggedIn + ', isLoggedIn: ' + isLoggedIn);

    if (!this.props.isReady) {
      if (isReady && !isLoggedIn) {
        auth0.showLogin();
      }
    }
  },

  componentWillMount() {
    AppState.addEventListener('change', (state) => {
      if (state === 'active') {
        console.log('[AppView] - Application state is now [active].');

        var loggedIn = store.getState().get('auth').get('isLoggedIn');

        if (this.props.isReady && loggedIn) {

          var userType = store.getState().get('auth').get('currentUser').get('userMetadata').get('userType');
          console.log('[AppView] - [' + userType  + '] type user is logged in and app is ready.');

          if (userType === 'elderly') {
            store.dispatch(HomepageStateActions.requestOneTimeNotification());
            console.log('[AppView] - Triggered a one time force fetch of [elder] data.');
          }
        }
      }

      if(state === 'background'){
        console.log('[AppView] - Application state is now [background].');
      }
    });
  },

  render() {
    if (!this.props.isReady) {
      return (
        <View>
          <ActivityIndicator style={styles.centered}/>
        </View>
      );
    }

    return (
      <View style={{flex: 1}}>
        <NavigationViewContainer router={AppRouter} />
        {__DEV__ && <DeveloperMenu />}
      </View>
    );
  }
});

const styles = StyleSheet.create({
  centered: {
    flex: 1,
    alignSelf: 'center'
  }
});

export default AppView;

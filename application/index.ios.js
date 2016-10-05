import {Provider} from 'react-redux';
import store from './src/redux/store';
import AppViewContainer from './src/modules/AppViewContainer';
import * as HomepageStateActions from './src/modules/homepage/HomepageState'
import * as HomepageCaregiverStateActions from './src/modules/homepage-caregiver/HomepageState'

import React from 'react';
import {AppRegistry} from 'react-native';

const Cami = React.createClass({

  render() {
    // Start polling for notifications.
    store.dispatch(HomepageStateActions.requestNotification());
    store.dispatch(HomepageCaregiverStateActions.requestCaregiverData());

    return (
      <Provider store={store}>
        <AppViewContainer />
      </Provider>
    );
  }
});

AppRegistry.registerComponent('Cami', () => Cami);

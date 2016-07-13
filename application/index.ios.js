import {Provider} from 'react-redux';
import store from './src/redux/store';
import AppViewContainer from './src/modules/AppViewContainer';
import HomepageViewContainer from './src/modules/homepage/HomepageViewContainer'

import React from 'react';
import {AppRegistry} from 'react-native';

const Cami = React.createClass({

  render() {
    return (
      <Provider store={store}>
        <HomepageViewContainer />
      </Provider>
    );
  }
});

AppRegistry.registerComponent('Cami', () => Cami);

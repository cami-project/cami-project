import {Map} from 'immutable'
import React, {PropTypes} from 'react';
import {
  StyleSheet,
  View,
  Image
} from 'react-native';

import variables from 'Cami/src/modules/variables/ElderGlobalVariables';

import * as LoginState from './LoginState';

var Color = require("color");

const LoginView = React.createClass({
  render() {
    return (
      <View style={variables.container}>
        <Image
          style={[styles.background, {zIndex: 1, resizeMode: 'cover'}]}
          source={require('../../../images/elder-bg-default.jpg')}
          />
        <View
          style={[
            styles.background,
            {
              zIndex: 2,
              backgroundColor: Color(variables.colors.status.low).clearer(.25).rgbaString()
            }
          ]}
          />
      </View>
    );
  }
});

const styles = StyleSheet.create({
  background: {
    position: 'absolute',
    flex: 1,
    width: variables.dimensions.width,
    height: variables.dimensions.height,
    top: 0,
    left: 0,
    alignSelf: 'center',
  }
});

export default LoginView;

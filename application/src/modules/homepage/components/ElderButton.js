import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
  TouchableOpacity
} from 'react-native';
import Color from 'color';
import variables from '../../variables/ElderGlobalVariables';

import * as HomepageState from '../HomepageState';

const ElderButton = React.createClass({
  propTypes: {
    type: PropTypes.string.isRequired,
    action: PropTypes.func.isRequired,
    severity: PropTypes.string.isRequired,
    text: PropTypes.string.isRequired
  },

  render() {
    return(
      <TouchableOpacity
        style={[
          styles.button,
          {
            shadowColor: Color(variables.colors.status[this.props.severity]).darken(.7).hexString(),
            backgroundColor: this.props.type === 'panic' ? variables.colors.panic : 'white'
          }
        ]}
        onPress={this.props.action}
      >
        <Text style={[
          styles.buttonText,
          {color: this.props.type === 'panic' ? 'white' : 'green'}
        ]}>{this.props.text}</Text>
      </TouchableOpacity>
    );
  }
});

const buttonCircle = {
  borderWidth: 0,
  borderRadius: 45,
  width: 90,
  height: 90
};

const styles = StyleSheet.create({
  button: {
    ...buttonCircle,
    alignSelf: 'center',
    justifyContent: 'center',
    shadowRadius: 40,
    shadowOffset: {width: 0, height: 0},
    shadowOpacity: 0.4,
  },
  buttonText: {
    backgroundColor: 'transparent',
    textAlign: 'center',
    alignSelf: 'center',
    fontSize: 22,
    fontWeight: 'bold'
  },
});

export default ElderButton;

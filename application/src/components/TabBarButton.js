import React, {PropTypes} from 'react';
import {
  Text,
  TouchableOpacity,
  StyleSheet
} from 'react-native';

import Icon from 'react-native-vector-icons/Ionicons';

var Color = require('color');

export default React.createClass({
  displayName: 'TabBarButton',
  propTypes: {
    text: PropTypes.string.isRequired,
    action: PropTypes.func.isRequired,
    isSelected: PropTypes.bool.isRequired,
    icon: PropTypes.string.isRequired
  },
  render() {
    return (
      <TouchableOpacity
        onPress={this.props.action}
        style={styles.button}
      >
        <Icon name={this.props.icon} size={22} color={this.props.isSelected ? color.active : color.inactive}/>
        <Text style={[styles.buttonText, this.props.isSelected && styles.selected]}>{this.props.text}</Text>
      </TouchableOpacity>
    );
  }
});

const color = {
  developing: '#a7b50a',
  active: '#00A4EE',
  inactive: Color('black').clearer(.25).rgbaString()
}

const styles = StyleSheet.create({
  button: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center'
  },
  buttonText: {
    fontSize: 12,
    color: color.inactive,
  },
  selected: {
    color: color.active
  }
});

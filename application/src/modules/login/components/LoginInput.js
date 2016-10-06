import React, {PropTypes} from 'react';
import {
  StyleSheet,
  View,
  TextInput
} from 'react-native';

import Icon from 'react-native-vector-icons/FontAwesome';
import icons from 'Cami/src/icons-fa';
import variables from 'Cami/src/modules/variables/ElderGlobalVariables';

var Color = require("color");

const LoginInput = React.createClass({
  propTypes: {
    placeholder: PropTypes.string.isRequired,
    icon: PropTypes.string.isRequired,
    secureTextEntry: PropTypes.bool.isRequired,
    name: PropTypes.string.isRequired,
    onTextChanged: PropTypes.func.isRequired
  },

  getInitialState() {
    return {
      inputBackgroundColor: 'transparent',
      inputColor: Color('white').clearer(.05).rgbaString()
    };
  },

  onBlur() {
    this.setState({
      inputBackgroundColor: 'transparent',
      inputColor: Color('white').clearer(.05).rgbaString()
    })
  },

  onFocus() {
    this.setState({
      inputBackgroundColor: Color('white').clearer(.05).rgbaString(),
      inputColor: Color(variables.colors.status.low).darken(.5).hexString()
    })
  },

  onTextChanged(text) {
    this.props.onTextChanged(text);
  },

  render() {
    return (
      <View style={styles.inputContainer}>
        <View style={styles.inputIcon}>
          <Icon
            name={icons[this.props.icon]}
            size={16}
            color={Color('white').clearer(.1).rgbaString()}
          />
        </View>
        <TextInput
          onChangeText={(text) => this.onTextChanged(text)}
          style={[
            styles.inputField,
            styles[this.props.name],
            {
              backgroundColor: this.state.inputBackgroundColor,
              color: this.state.inputColor
            }
          ]}
          placeholder={this.props.placeholder}
          placeholderTextColor={Color(this.state.inputColor).clearer(.45).rgbaString()}
          secureTextEntry={this.props.secureTextEntry}
          autoCapitalize="none"
          autoCorrect={false}
          onFocus={() => this.onFocus()}
          onBlur={() => this.onBlur()}
        />
      </View>
    );
  }
});

const styles = StyleSheet.create({
  inputContainer: {
    backgroundColor: 'transparent',
    marginBottom: 20,
    position: 'relative',
    alignItems: 'center'
  },
  inputIcon: {
    position: 'absolute',
    top: 10,
    left: -40,
    width: 40,
    alignItems: 'center'
  },
  inputField: {
    borderWidth: 1,
    borderColor: Color('white').clearer(.05).rgbaString(),
    borderStyle: 'solid',
    width: (variables.dimensions.width - 80),
    padding: 10,
    fontSize: 14,
    fontWeight: 'bold',
    lineHeight: 16,
    height: 36,
  },
});

export default LoginInput;

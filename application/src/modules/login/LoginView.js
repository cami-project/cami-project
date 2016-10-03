import {Map} from 'immutable'
import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
  Image,
  TouchableOpacity
} from 'react-native';

import LoginInput from './components/LoginInput';

import Icon from 'react-native-vector-icons/FontAwesome';
import icons from 'Cami/src/icons-fa';
import variables from 'Cami/src/modules/variables/ElderGlobalVariables';

import * as LoginState from './LoginState';

var Color = require("color");

var Sound = require('react-native-sound');
var tapButtonSound = new Sound('sounds/knuckle.mp3', Sound.MAIN_BUNDLE, (error) => {
  if (error) {
    console.log('failed to load the sound', error);
  }
});

const LoginView = React.createClass({
  username: "",
  password: "",

  propTypes: {
    dispatch: PropTypes.func.isRequired
  },
  didChangeUsername(username) {
    this.username = username;
  },
  didChangePassword(password) {
    this.password = password;
  },
  logIn() {
    tapButtonSound.setVolume(1.0).play();
    this.props.dispatch(LoginState.logIn(this.username, this.password));
  },
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
        <View style={styles.headerContainer}>
          <View style={styles.iconContainer}>
            <Icon
              name={icons.heart_solid}
              size={80}
              color={Color('white').clearer(.25).rgbaString()}
              />
          </View>
          <Text style={styles.greeting}>Welcome</Text>
        </View>

        <View style={styles.mainContainer}>
          <View style={styles.formContainer}>
            <LoginInput
              placeholder="Username"
              icon="user"
              secureTextEntry={false}
              name="username"
              onTextChanged={this.didChangeUsername}
              />
            <LoginInput
              placeholder="Password"
              icon="password"
              secureTextEntry={true}
              name="password"
              onTextChanged={this.didChangePassword}
              />
          </View>
          <TouchableOpacity
            style={[
              styles.button,
              styles.buttonConfirm,
              {
                shadowColor: Color(variables.colors.status.low).darken(.8).hexString()
              }
            ]}
            onPress={this.logIn}
            >
            <Text style={styles.buttonText}>
              LOGIN
            </Text>
          </TouchableOpacity>
        </View>
      </View>
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
  background: {
    position: 'absolute',
    flex: 1,
    width: variables.dimensions.width,
    height: variables.dimensions.height,
    top: 0,
    left: 0,
    alignSelf: 'center',
  },
  headerContainer: {
    flex: 1,
    zIndex: 5,
    alignItems: 'center',
    justifyContent: 'flex-end',
    paddingTop: 20,
    backgroundColor: 'transparent'
  },
  iconContainer: {
    backgroundColor: 'transparent',
    marginBottom: 20
  },
  greeting: {
    fontSize: 28,
    color: Color('white').clearer(.1).rgbaString(),
    backgroundColor: 'transparent'
  },
  mainContainer: {
    flex: 2,
    alignItems: 'center',
    zIndex: 4,
    backgroundColor: 'transparent',
    marginTop: 50
  },
  formContainer: {
    marginBottom: 30
  },
  buttonContainer: {
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center'
  },
  button: {
    ...buttonCircle,
  alignSelf: 'center',
  justifyContent: 'center',
  shadowRadius: 40,
  shadowOffset: {width: 0, height: 0},
  shadowOpacity: 0.5,
  backgroundColor: 'white'
  },
buttonText: {
  backgroundColor: 'transparent',
    textAlign: 'center',
      alignSelf: 'center',
        fontSize: 16,
          fontWeight: 'bold',
            color: Color(variables.colors.status.low).clearer(.05).rgbaString()
}
});

export default LoginView;

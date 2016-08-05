import {Map} from 'immutable'
import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
  Image,
  TouchableOpacity
} from 'react-native';

import Icon from 'react-native-vector-icons/FontAwesome';
import icons from 'Cami/src/icons-fa';
import variables from 'Cami/src/modules/variables/ElderGlobalVariables';

var Color = require("color");

const HomepageView = React.createClass({
  propTypes: {
    notification: PropTypes.instanceOf(Map).isRequired,
    username: PropTypes.string.isRequired
  },

  render() {
    return (
      <View style={styles.container}>

        <View style={styles.iconContainer}>
          <View style={styles.outerRing}>
            <View style={styles.iconRing}>
              <Icon
                name={icons[this.props.notification.get('type')]}
                size={50}
                color={variables.colors.status[this.props.notification.get('severity')]}
                style={{
                  alignSelf: 'center',
                  marginTop: -5
                }}
              />
            </View>
          </View>
        </View>

        <View style={styles.mainContainer}>
          <View style={styles.textContainer}>
            <Text style={[styles.text, {fontWeight: 'bold'}]}>
              Hey {this.props.username}
            </Text>
            <Text style={[styles.text, {paddingTop: 20}]}>
              {this.props.notification.get("message")}
            </Text>
          </View>

          <View style={styles.buttonContainer}>
            <TouchableOpacity
              style={[
                styles.button,
                styles.buttonPanic,
                {
                  shadowColor: Color(variables.colors.status[this.props.notification.get('severity')]).darken(.7).hexString()
                }
              ]}
            >
              <Text style={[styles.buttonText, {color: 'white'}]}>
                Help
              </Text>
            </TouchableOpacity>

            <TouchableOpacity
              style={[
                styles.button,
                styles.buttonConfirm,
                {
                  shadowColor: Color(variables.colors.status[this.props.notification.get('severity')]).darken(.7).hexString()
                }
              ]}
            >
              <Text style={[styles.buttonText, {color: 'green'}]}>
                OK
              </Text>
            </TouchableOpacity>
          </View>
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
  container: {
    flex: 1,
  },
  iconContainer: {
    flex: 1,
    zIndex: 2,
    alignItems: 'center',
    justifyContent: 'center',
    paddingTop: 20
  },
  outerRing: {
    borderWidth: 2,
    borderRadius: 72,
    width: 140,
    height: 140,
    borderColor: Color('white').clearer(.75).rgbaString(),
    marginBottom: -70,
    justifyContent: 'center'
  },
  iconRing: {
    borderWidth: 0,
    borderRadius: 60,
    width: 120,
    height: 120,
    backgroundColor: Color('white').clearer(.25).rgbaString(),
    backgroundColor: Color('white').clearer(.05).rgbaString(),
    alignSelf: 'center',
    justifyContent: 'center'
  },
  mainContainer: {
    flex: 3,
    alignItems: 'center',
    zIndex: 1
  },
  textContainer: {
    paddingTop: 40,
    width: variables.dimensions.width*.8,
    justifyContent: 'center'
  },
  text: {
    fontSize: 26,
    color: variables.colors.text,
    lineHeight: 1.3*26
  },
  buttonContainer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    width: variables.dimensions.width*.8,
    position: 'absolute',
    bottom: variables.dimensions.width*.1,
    left: variables.dimensions.width*.1
  },
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
  buttonConfirm: {
    backgroundColor: 'white',
  },
  buttonPanic: {
    backgroundColor: variables.colors.panic
  }
});

export default HomepageView;

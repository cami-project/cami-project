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
import Sound from 'react-native-sound';
import Color from 'color';
import icons from 'Cami/src/icons-fa';
import variables from 'Cami/src/modules/variables/ElderGlobalVariables';
import ElderButton from './components/ElderButton';

const tapButtonSound = new Sound('sounds/knuckle.mp3', Sound.MAIN_BUNDLE, (error) => {
  if (error) {
    console.log('failed to load the sound', error);
  }
});

const HomepageView = React.createClass({
  propTypes: {
    notification: PropTypes.instanceOf(Map).isRequired,
    username: PropTypes.string.isRequired,
    logout: PropTypes.func.isRequired
  },

  // some Journal entries don't come w/ severity information
  // - for the elder's interface it's essential to have custom color schemes
  //   based on the severity, to improve the way he/she'll react to it
  matchSeverity() {
    severity = this.props.notification.get('severity');

    if (severity !== 'none') {
      return severity;
    } else {
      // this will be the case for activity-related entries
      switch (this.props.notification.get('type')) {
        case 'exercise': return 'low';
        case 'medication': return 'medium';
        case 'appointment': return 'low';
        default: return 'low';
      }
    }
  },

  render() {
    return (
      <View style={styles.container}>
        <Image
          style={[styles.background, {zIndex: 1, resizeMode: 'cover'}]}
          source={require('../../../images/elder-bg-default.jpg')}
        />
        <View
          style={[
            styles.background,
            {
              zIndex: 2,
              backgroundColor: Color(variables.colors.status[this.matchSeverity()]).clearer(.25).rgbaString()
            }
          ]}
        />
        <Image
          style={[styles.background, {zIndex: 3, resizeMode: 'contain'}]}
          source={require('../../../images/elder-mainContent-bg.png')}
        />
        <View style={styles.iconContainer}>
          <View style={styles.logoutButtonContainer}>
            <TouchableOpacity
              style={styles.logoutButton}
              onPress={() => tapButtonSound.setVolume(1.0).play() && this.props.logout()}
            >
              <Icon
                name={icons.logout}
                size={16}
                color={Color('white').clearer(.25).rgbaString()}
              />
            </TouchableOpacity>
          </View>
          <View style={styles.outerRing}>
            <View style={styles.iconRing}>
              <Icon
                name={icons[this.props.notification.get('type')]}
                size={60}
                color={variables.colors.status[this.matchSeverity()]}
                style={{
                  alignSelf: 'center',
                  marginTop: -10
                }}
              />
            </View>
          </View>
        </View>

        <View style={[
            styles.mainContainer,
          ]}
        >
          <View style={styles.textContainer}>
            <Text style={[styles.text, {fontWeight: 'bold'}]}>
              Hey Jim
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
                  shadowColor: Color(variables.colors.status[this.matchSeverity()]).darken(.7).hexString()
                }
              ]}
              onPress={() => tapButtonSound.setVolume(1.0).play()}
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
                  shadowColor: Color(variables.colors.status[this.matchSeverity()]).darken(.7).hexString()
                }
              ]}
              onPress={() => tapButtonSound.setVolume(1.0).play()}
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
    position: 'relative'
  },
  background: {
    position: 'absolute',
    flex: 1,
    width: variables.dimensions.width,
    height: variables.dimensions.height,
    top: 0,
    left: 0,
    alignSelf: 'center',
  },
  iconContainer: {
    flex: 1,
    zIndex: 5,
    alignItems: 'center',
    justifyContent: 'center',
    paddingTop: 20,
    position: 'relative'
  },
  logoutButtonContainer: {
    position: 'absolute',
    top: 25,
    left: 20,
    backgroundColor: 'transparent'
  },
  outerRing: {
    borderWidth: 2,
    borderRadius: 90,
    width: 180,
    height: 180,
    borderColor: Color('white').clearer(.75).rgbaString(),
    marginBottom: -75,
    justifyContent: 'center'
  },
  iconRing: {
    borderWidth: 0,
    borderRadius: 75,
    width: 150,
    height: 150,
    backgroundColor: Color('white').clearer(.05).rgbaString(),
    alignSelf: 'center',
    justifyContent: 'center'
  },
  mainContainer: {
    flex: 3,
    alignItems: 'center',
    zIndex: 4,
    backgroundColor: 'transparent',
    marginTop: 50
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

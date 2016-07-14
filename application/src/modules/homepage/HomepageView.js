import {Map} from 'immutable'
import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
  Image,
  TouchableOpacity,
  Dimensions
} from 'react-native';

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
              <Text
                style={{
                  backgroundColor: 'transparent',
                  alignSelf: 'center',
                  color: '#5a5a5a'
                }}
              >ICON</Text>
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
            <TouchableOpacity style={[styles.button, styles.buttonPanic]}>
              <Text style={[styles.buttonText, {color: 'white'}]}>
                Help
              </Text>
            </TouchableOpacity>

            <TouchableOpacity style={[styles.button, styles.buttonConfirm]}>
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

const username = 'Jim';

let {height, width} = Dimensions.get('window');

const color = {
  developing: '#a7b50a'
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    paddingTop: 20,
    backgroundColor: 'moccasin'
  },
  iconContainer: {
    flex: 1,
    backgroundColor: '#eaeaea',
    zIndex: 2,
    alignItems: 'center',
    justifyContent: 'center'
  },
  outerRing: {
    borderWidth: 2,
    borderRadius: 72,
    width: 140,
    height: 140,
    borderColor: 'rgba(255,255,255,0.35)',
    marginBottom: -70,
    justifyContent: 'center'
  },
  iconRing: {
    borderWidth: 0,
    borderRadius: 60,
    width: 120,
    height: 120,
    backgroundColor: 'white',
    alignSelf: 'center',
    justifyContent: 'center'
  },
  mainContainer: {
    flex: 3,
    backgroundColor: '#dbdbdb',
    alignItems: 'center',
    zIndex: 1
  },
  textContainer: {
    paddingTop: 40,
    width: width*.8,
    justifyContent: 'center'
  },
  text: {
    fontSize: 26,
    color: '#158A12',
    lineHeight: 1.3*26
  },
  buttonContainer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    width: width*.8,
    position: 'absolute',
    bottom: width*.1,
    left: width*.1
  },
  button: {
    ...buttonCircle,
    alignSelf: 'center',
    justifyContent: 'center',
    shadowRadius: 40,
    shadowOffset: {width: 0, height: 0},
    shadowOpacity: 0.35,
    shadowColor: color.developing
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
    backgroundColor: '#C95F5F'
  }
});

export default HomepageView;

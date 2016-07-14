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

const styles = StyleSheet.create({});

export default HomepageView;

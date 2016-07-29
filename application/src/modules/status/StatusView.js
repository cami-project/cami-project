import {List} from 'immutable';
import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
  ScrollView,
} from 'react-native';
import Color from 'color';
import moment from 'moment';

// import StatusEntry from './components/StatusEntry';

const DATE_FORMAT = 'D MMM';
const WEEK_DATE_FORMAT = 'ddd D MMM';

const StatusView = React.createClass({
  propTypes: {
    username: PropTypes.string.isRequired
  },

  render() {
    return (
      <View style={styles.container}>
        <View style={styles.iconContainer}>
          <Text style={[styles.mainText, {fontWeight: 'bold'}]}>
            Status
          </Text>
        </View>

        <ScrollView>
        </ScrollView>
      </View>
    );
  }
});

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: 'moccasin'
  },
  iconContainer: {
    // flex: 1,
    backgroundColor: '#658d51',
    zIndex: 2,
    alignItems: 'center',
    justifyContent: 'center',
    paddingTop: 20
  },
  mainText: {
    fontSize: 26,
    color: 'white',
    lineHeight: 1.3*26
  },
});

export default StatusView;

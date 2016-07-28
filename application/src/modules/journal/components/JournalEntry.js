import {List} from 'immutable'
import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
  Image
} from 'react-native';

import moment from 'moment';

import {images} from 'Cami/src/images';

const chartStyles = StyleSheet.create({
  container: {
    flex: 1
  },
  chartContainer: {
    // flex: 1,
    // backgroundColor: 'white',
  },
  chart: {
    height: 50,
  },
  value: {
    fontSize: 26,
    color: 'black',
    lineHeight: 1.3*26
  },
  unit: {
    fontSize: 16,
    color: 'black',
    lineHeight: 1.3*26
  },
  description: {
    fontSize: 18,
    color: 'black',
    lineHeight: 1.3*26
  }
});

const JournalEntry = React.createClass({
  propTypes: {
    type: PropTypes.string.isRequired,
    status: PropTypes.string.isRequired,
    timestamp: PropTypes.number.isRequired,
    title: PropTypes.string.isRequired,
    message: PropTypes.string.isRequired
  },

  render() {
    const time = moment(new Date(this.props.timestamp * 1000)).format('HH mm');

    return (
      <View style={{backgroundColor: 'white', flexDirection: 'row'}}>
        <View style={{flex: 1}}>
          <View style={{justifyContent: 'center', alignItems: 'center'}}>
            <Image source={images[this.props.type][this.props.status]}/>
          </View>
          <View style={{justifyContent: 'center', alignItems: 'center'}}>
            <Text>{time}</Text>
          </View>
        </View>
        <View style={{flex: 3, justifyContent: 'center', alignItems: 'center'}}>
          <Text>{this.props.title}</Text>
          <Text>{this.props.message}</Text>
        </View>
      </View>
    );
  }
});

export default JournalEntry;

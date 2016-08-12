import {Map} from 'immutable';
import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
  ScrollView,
} from 'react-native';

import HeartRateEntry from './components/HeartRateEntry';
import WeightEntry from './components/WeightEntry';
import StepsEntry from './components/StepsEntry';
import SleepEntry from './components/SleepEntry';
import BloodPressureEntry from './components/BloodPressureEntry';

const StatusView = React.createClass({
  propTypes: {
    username: PropTypes.string.isRequired,
    status: PropTypes.instanceOf(Map).isRequired
  },

  getEntryByType(type) {
    switch(type) {
      case 'heart': return HeartRateEntry;
      case 'weight': return WeightEntry;
      case 'steps': return StepsEntry;
      case 'sleep': return SleepEntry;
      case 'blood': return BloodPressureEntry;
    }
  },

  render() {
    const entries = [];
    this.props.status.forEach((status, index) => {
      const EntryType = this.getEntryByType(index);
      if (EntryType)
        entries.push(<EntryType key={index} statusItem={status}/>);
    });

    return (
      <View style={styles.container}>
        <View style={styles.iconContainer}>
          <Text style={[styles.mainText, {fontWeight: 'bold'}]}>
            Status
          </Text>
        </View>

        <ScrollView>
          {entries}
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

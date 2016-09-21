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

import variables from '../variables/CaregiverGlobalVariables';

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
        entries.push(<EntryType style={styles.statusEntry} key={index} statusItem={status}/>);
    });

    return (
      <View style={variables.container}>
        <ScrollView style={styles.statusContainer}>
          {entries}
        </ScrollView>
      </View>
    );
  }
});

const styles = StyleSheet.create({
  statusContainer: {
    flex: 1,
    position: 'relative',
    marginTop: 20,
    marginBottom: 10,
    paddingLeft: 10,
    paddingRight: 10,
    backgroundColor: 'transparent'
  }
});

export default StatusView;

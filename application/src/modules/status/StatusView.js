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
    status: PropTypes.instanceOf(Map).isRequired,
    weight: PropTypes.instanceOf(Map).isRequired,
    heart_rate: PropTypes.instanceOf(Map).isRequired,
    steps: PropTypes.instanceOf(Map).isRequired,
    bp_diastolic: PropTypes.instanceOf(Map).isRequired,
    bp_systolic: PropTypes.instanceOf(Map).isRequired,
    bp_pulse: PropTypes.instanceOf(Map).isRequired,
    bp_aggregated: PropTypes.instanceOf(Map).isRequired
  },

  getEntryByType(type) {
    switch (type) {
      case 'sleep': return SleepEntry;
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
          {
            this.props.bp_aggregated.get('data').size > 0
              && this.props.bp_systolic.get('data').size > 0
              && this.props.bp_diastolic.get('data').size > 0
              && this.props.bp_pulse.get('data').size > 0
            ?
              <BloodPressureEntry
                style={styles.statusEntry}
                key={100}
                aggregated={this.props.bp_aggregated}
                diastolic={this.props.bp_diastolic}
                systolic={this.props.bp_systolic}
                pulse={this.props.bp_pulse}
              />
            :
              null
          }
          {
            this.props.weight.get('data').size > 0
            ?
              <WeightEntry style={styles.statusEntry} key={101} weight={this.props.weight}/>
            :
              null
          }
          {
            this.props.heart_rate.get('data').size > 0
            ?
              <HeartRateEntry style={styles.statusEntry} key={102} heart={this.props.heart_rate}/>
            :
              null
          }
          {
            this.props.steps.get('data').size > 0
            ?
              <StepsEntry style={styles.statusEntry} key={103} steps={this.props.steps}/>
            :
              null
          }
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
    marginTop: 30,
    paddingLeft: 10,
    paddingRight: 10,
    backgroundColor: 'transparent'
  }
});

export default StatusView;

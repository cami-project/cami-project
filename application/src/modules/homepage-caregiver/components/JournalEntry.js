import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
  Image
} from 'react-native';
import Icon from 'react-native-vector-icons/FontAwesome';
import moment from 'moment';
import icons from 'Cami/src/icons-fa';
import variables from '../../variables/CaregiverGlobalVariables';

const JournalEntry = React.createClass({
  propTypes: {
    type: PropTypes.string.isRequired,
    status: PropTypes.string.isRequired,
    timestamp: PropTypes.number.isRequired,
    title: PropTypes.string.isRequired
  },

  matchStatus() {
    var status = this.props.status;

    if (status !== 'none') {
      return status;
    } else {
      // this will be the case for activity-related entries
      switch (this.props.type) {
        case 'exercise': return 'info';
        case 'medication': return 'info';
        case 'appointment': return 'info';
        default: return 'info';
      }
    }
  },

  render() {
    const time = moment(new Date(this.props.timestamp * 1000)).utc().format('HH:mm');

    return (
      <View style={styles.journalEntry}>
        <View style={styles.icon}>
          <Icon name={icons[this.props.type]} size={20} color={variables.colors.status[this.matchStatus()]}/>
        </View>
        <View>
          <Text style={styles.time}>{time}</Text>
        </View>
        <View style={{flex: 1}}>
          <Text style={styles.text}>{this.props.title}</Text>
        </View>
      </View>
    );
  }
});

const styles = StyleSheet.create({
  journalEntry: {
    backgroundColor: 'white',
    padding: 10,
    marginBottom: 10,
    flexDirection: 'row',
    justifyContent: 'flex-start',
    alignItems: 'center'
  },
  icon: {
    paddingRight: 10
  },
  time: {
    fontSize: 10,
    fontWeight: 'bold',
    paddingRight: 10,
    color: variables.colors.gray.neutral
  },
  text: {
    color: variables.colors.gray.darker,
    fontSize: 12
  }
});

export default JournalEntry;

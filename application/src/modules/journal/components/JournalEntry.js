import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View
} from 'react-native';

import moment from 'moment';
import Icon from 'react-native-vector-icons/FontAwesome';
import icons from 'Cami/src/icons-fa';
import variables from '../../variables/CaregiverGlobalVariables';

import {images} from 'Cami/src/images';

const styles = StyleSheet.create({
  journalEntry: {
    backgroundColor: 'white',
    padding: 10,
    marginBottom: 10,
    flexDirection: 'row',
    justifyContent: 'flex-start',
    alignItems: 'center',
    borderTopWidth: 2
  },
  iconContainer: {
    paddingRight: 20,
    paddingLeft: 10,
    paddingTop: 10,
    paddingBottom: 10
  },
  icon: {
    alignItems: 'center'
  },
  time: {
    fontSize: 12,
    fontWeight: 'bold',
    paddingTop: 10,
    color: variables.colors.gray.neutral
  },
  text: {
    color: variables.colors.gray.darker,
    fontSize: 12
  },
  actionMessage: {
    paddingTop: 10,
    marginTop: 10,
    borderTopWidth: 10,
    borderTopColor: variables.colors.gray.dark
  },
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
    const time = moment(new Date(this.props.timestamp * 1000)).format('HH:mm');

    return (
      <View style={[styles.journalEntry, {borderColor: variables.colors.status[this.props.status]}]}>
        <View style={styles.iconContainer}>
          <View style={styles.icon}>
            <Icon name={icons[this.props.type]} size={30} color={variables.colors.status[this.props.status]}/>
          </View>
          <View>
            <Text style={styles.time}>{time}</Text>
          </View>
        </View>
        <View style={{flex: 1}}>
          <Text style={[styles.text, styles.statusMessage]}>{this.props.title}</Text>
          <Text style={[styles.text, styles.actionMessage]}>{this.props.message}</Text>
        </View>
      </View>
    );
  }
});

export default JournalEntry;

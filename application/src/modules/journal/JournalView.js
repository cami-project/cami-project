import {List} from 'immutable'
import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
  ScrollView,
  Image,
  TouchableOpacity,
  Dimensions,
  Component
} from 'react-native';
import Color from 'color';
import moment from 'moment';

import JournalEntry from './components/JournalEntry';
import variables from '../variables/CaregiverGlobalVariables';

const DATE_FORMAT = 'D MMM';
const WEEK_DATE_FORMAT = 'ddd D MMM';

const JournalView = React.createClass({
  propTypes: {
    events: PropTypes.instanceOf(List).isRequired,
    username: PropTypes.string.isRequired
  },

  render() {
    // TODO switch places the following 2 lines
    // const todayDateText = moment().format(DATE_FORMAT);
    const todayDateText = moment(new Date(this.props.events.get(0).get('timestamp') * 1000)).format(DATE_FORMAT);
    const firstEventDateText = moment(new Date(this.props.events.get(0).get('timestamp') * 1000)).format(DATE_FORMAT);
    const headerDateText = todayDateText == firstEventDateText ? 'Today ' + todayDateText : firstEventDateText;

    const events = [];
    let dayKey = firstEventDateText;
    this.props.events.forEach((event, index) => {
      const day = moment(new Date(event.get('timestamp') * 1000)).format(DATE_FORMAT);
      if (day != dayKey) {
        dayKey = day;
        const weekDayText = moment(new Date(event.get('timestamp') * 1000)).format(WEEK_DATE_FORMAT);
        events.push(
          <View key={'text' + index} style={[styles.dateContainer, {flex: 1}]}>
            <View style={styles.dateRuler}><View style={styles.dateBullet}/></View>
            <Text style={[styles.date]}>{weekDayText}</Text>
          </View>
        );
      }
      events.push(
        <JournalEntry
          key={'entry' + index}
          type={event.get('type')}
          status={event.get('status')}
          timestamp={event.get('timestamp')}
          title={event.get('title')}
          message={event.get('message')}
        />
      );
    });

    return (
      <View style={variables.container}>
        <View style={styles.headerContainer}>
          <Image style={styles.avatar} source={require('../../../images/old-man.jpg')}/>
          <Text style={styles.headerDate}>
            {headerDateText}
          </Text>
        </View>

        <View style={styles.timeline}></View>

        <ScrollView style={styles.journalContainer}>
          {events}
        </ScrollView>
      </View>
    );
  }
});

const styles = StyleSheet.create({
  headerContainer: {
    backgroundColor: variables.colors.gray.lightest,
    borderBottomWidth: 1,
    borderColor: variables.colors.gray.light,
    height: 70,
    zIndex: 6,
    alignItems: 'center',
    justifyContent: 'center',
    position: 'relative',
  },
  avatar: {
    backgroundColor: 'transparent',
    borderRadius: 30,
    width: 60,
    height: 60,
    borderWidth: 4,
    borderColor: variables.colors.gray.light,
    position: 'absolute',
    bottom: -30,
    marginLeft: 60,
    zIndex: 6
  },
  dateContainer: {
    position: 'relative'
  },
  headerDate: {
    position: 'absolute',
    right: 10,
    bottom: 10,
    fontSize: 12
  },
  dateRuler: {
    width: variables.dimensions.width-20,
    position: 'absolute',
    height: 2,
    backgroundColor: variables.colors.gray.light,
    top: 6,
    left: -10
  },
  dateBullet: {
    position: 'absolute',
    width: 8,
    height: 8,
    borderRadius: 4,
    backgroundColor: variables.colors.gray.light,
    top: -3,
    left: 87
  },
  date: {
    fontSize: 12,
    backgroundColor: variables.colors.background,
    color: variables.colors.gray.neutral,
    position: 'relative',
    marginBottom: 20,
    width: 70,
    alignSelf: 'flex-end',
    textAlign: 'right'
  },
  journalContainer: {
    flex: 1,
    position: 'relative',
    paddingTop: 50,
    backgroundColor: 'transparent',
    paddingLeft: 10,
    paddingRight: 10,
    zIndex: 5
  },
  timeline: {
    width: 2,
    height: variables.dimensions.height,
    backgroundColor: variables.colors.gray.light,
    position: 'absolute',
    left: 90,
    zIndex: 2
  },
});

export default JournalView;

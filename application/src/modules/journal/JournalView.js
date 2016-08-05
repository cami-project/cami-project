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
        events.push(<Text key={'text' + index} style={{textAlign: 'right'}}>{weekDayText}</Text>);
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
          <Image style={styles.avatar} source={require('../../../images/old-man.png')}/>
          <Text style={styles.date}>
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
    zIndex: 4,
    alignItems: 'center',
    justifyContent: 'center',
    position: 'relative',
  },
  avatar: {
    borderWidth: 0,
    borderRadius: 40,
    width: 80,
    height: 80,
    backgroundColor: Color('white').clearer(.25).rgbaString(),
    borderWidth: 5,
    borderColor: 'white',
    position: 'absolute',
    bottom: -40,
    marginLeft: variables.dimensions.width/2 - 40,
    zIndex: 5
  },
  date: {
    position: 'absolute',
    right: 10,
    bottom: 10,
    fontSize: 12
  },
  journalContainer: {
    flex: 1,
    position: 'relative',
    marginTop: 60,
    backgroundColor: 'transparent',
    paddingLeft: 10,
    paddingRight: 10,
    zIndex: 5
  },
  timeline: {
    width: 4,
    height: variables.dimensions.height,
    backgroundColor: 'white',
    position: 'absolute',
    left: variables.dimensions.width/2 - 2,
    zIndex: 2
  },
});

export default JournalView;

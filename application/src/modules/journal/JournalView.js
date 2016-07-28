import {List} from 'immutable'
import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
  Image,
  TouchableOpacity,
  Dimensions,
  Component
} from 'react-native';
import Color from 'color';
import moment from 'moment';

import JournalEntry from './components/JournalEntry';

const DATE_FORMAT = 'D MMM';

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
      console.log('day:', day);
      if (day != dayKey) {
        events.push(<Text key={'text' + index}>{day}</Text>);
        dayKey = day;
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
      <View style={styles.container}>
        <View style={styles.iconContainer}>
          <Text style={[styles.mainText, {fontWeight: 'bold'}]}>
            Journal
          </Text>
          <View style={styles.outerRing}>
            <Image style={styles.iconRing} source={require('../../../images/old-man.png')}/>
          </View>
          <Text style={[styles.mainText, {textAlign: 'right', zIndex: 1}]}>
            {headerDateText}
          </Text>
        </View>

        {events}
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
    flex: 1,
    backgroundColor: '#658d51',
    zIndex: 2,
    alignItems: 'center',
    justifyContent: 'center',
    // paddingTop: 20
  },
  outerRing: {
    borderWidth: 2,
    borderRadius: 72,
    width: 100,
    height: 100,
    borderColor: Color('white').clearer(.75).rgbaString(),
    marginBottom: -70,
    justifyContent: 'center'
  },
  iconRing: {
    borderWidth: 0,
    borderRadius: 60,
    width: 80,
    height: 80,
    backgroundColor: Color('white').clearer(.25).rgbaString(),
    alignSelf: 'center',
    justifyContent: 'center'
  },
  mainContainer: {
    flex: 3,
    backgroundColor: '#dbdbdb',
    // alignItems: 'center',
    zIndex: 1
  },
  mainText: {
    fontSize: 26,
    color: 'white',
    lineHeight: 1.3*26
  },
});

export default JournalView;

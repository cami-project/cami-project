import {List} from 'immutable';
import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
  ScrollView,
  Dimensions,
  Component
} from 'react-native';

// Utilities
import Color from 'color';
import moment from 'moment';
import Icon from 'react-native-vector-icons/FontAwesome';
import variables from '../variables/CaregiverGlobalVariables';
import icons from 'Cami/src/icons-fa';

import ActivityEntry from './components/ActivityEntry';

const ActivitiesView = React.createClass({
  propTypes: {
    events: PropTypes.instanceOf(List).isRequired,
    username: PropTypes.string.isRequired,
  },

  componentDidMount() {
    if(this.props.events.size > 0) {
      const _scrollView = this.scrollView;
      // we need the nextEventId to calculate scrolling distance
      const nextEventId = this.findNextEventId(this.props.events, moment());
      // scroll offset to get last event in view
      // - used when there aren't enough future events to justify nextEvent hidden under header
      const scrollToEndOffset = ((this.props.events.size * 127) - (variables.dimensions.height - 140 - 49 - 20));
      // scroll offset to get next event hidden under header
      // - only applies if there are a sufficient amount of future events (~4)
      const scrollToEventOffset = (127 * (nextEventId + 1));

      _scrollView.scrollTo({
        x: 0,
        y: (this.props.events.size - (nextEventId + 1)) < 4 ? scrollToEndOffset : scrollToEventOffset,
        animated: true
      });
    }
  },

  findNextEventId(events, today) {
    const futureEvents = [];
    events.forEach((event, index) => {
        if(moment(event.get('start')).isSameOrAfter(today.unix())) {
          futureEvents.push(index);
        }
    });
    return futureEvents[0];
  },

  render() {
    const today = moment();
    // placeholder for the events to be itterated upon
    const events = [];
    // id of next event from the list of events received form the API
    // - also used to decide which events are to be displayed as archived
    const nextEventId = this.findNextEventId(this.props.events, today);
    // moment of next event, used for header dates formatting
    const nextEventDate = moment.unix(this.props.events.get(nextEventId).get('start'));
    // month name of 1st event that will be used to selectively display month separators
    let monthKey = moment.unix(this.props.events.get(0).get('start')).format('MMMM');

    this.props.events.forEach((event, index) => {
      // every time a month changes we show a visual separator inside the timeline
      const month = moment.unix(event.get('start')).format('MMMM');
      if (month !== monthKey) {
        monthKey = month;
        events.push(
          <View key={'text' + index} style={[styles.dateContainer, {flex: 1}]}>
            <View style={styles.dateRuler}><View style={styles.dateBullet}/></View>
            <Text style={[styles.date]}>{month}</Text>
          </View>
        );
      }

      // and now let's build that event list
      // - TODO(@rtud): color gets passed as hex value for now
      //   - we should create a local mappging of gCal registered colors
      events.push(
        <ActivityEntry
          key={'entry' + index}
          timestamp={event.get('start')}
          title={event.get('title')}
          description={event.get('description') !== null ? event.get('description') : 'No description set'}
          location={event.get('location') !== null ? event.get('location') : 'No location set'}
          color={event.get('color').get('background')}
          archived={index < nextEventId ? true : false}
          today={index !== nextEventId ? false : true}
        />
      );
    });

    return (
      <View style={variables.container}>
        <View style={styles.headerContainer}>
          <View style={styles.nextEventDate}>
            <View>
              {
                nextEventDate
                  ? <Text style={styles.dayName}>{nextEventDate.format('ddd')}</Text>
                  : <Text style={styles.dayName}>--</Text>
              }
            </View>
            <View>
              {
                nextEventDate
                  ? <Text style={styles.day}>{nextEventDate.format('DD')}</Text>
                  : <Text style={styles.day}>--</Text>
              }
            </View>
            <View>
              {
                nextEventDate
                  ? <Text style={styles.month}>{nextEventDate.format('MMM').toUpperCase()}</Text>
                  : <Text style={styles.month}>---</Text>
              }
            </View>
          </View>

          <View style={styles.nextEventInformation}>
            <View style={styles.nextEventInformationHeading}>
              <View>
                <Text style={styles.nextText}>Next</Text>
              </View>
              <View>
                <Text style={styles.todayText}>{'Today ' + today.format('DD MMM')}</Text>
              </View>
            </View>
            {
              this.props.events.get(nextEventId).get('title') !== null
                ? <Text style={styles.nextTitle}>{this.props.events.get(nextEventId).get('title')}</Text>
                : <Text style={[styles.nextTitle, {color: variables.colors.gray.neutral}]}>No title set</Text>
            }
            {
              this.props.events.get(nextEventId).get('description') !== null
                ? <Text style={styles.nextDescription}>{this.props.events.get(nextEventId).get('description')}</Text>
                : <Text style={[styles.nextDescription, {color: variables.colors.gray.neutral}]}>No description set</Text>
            }
            <View style={styles.nextMeta}>
              <Icon
                name={icons.time}
                size={14}
                color={variables.colors.gray.neutral}
                style={{paddingRight: 5}}
              />
              <View style={styles.nextTime}>
                {
                  nextEventDate
                    ? <Text style={styles.metaText}>{nextEventDate.format('HH:mm')}</Text>
                    : <Text style={styles.metaText}>--:--</Text>
                }
              </View>
              <Icon
                name={icons.location}
                size={14}
                color={variables.colors.gray.neutral}
                style={{paddingRight: 5, marginLeft: 10}}
              />
              <View style={styles.nextLocation}>
                {
                  this.props.events.get(nextEventId).get('location') !== null
                    ? <Text style={styles.metaText}>{this.props.events.get(nextEventId).get('location')}</Text>
                    : <Text style={styles.metaText}>No location set</Text>
                }
              </View>
            </View>
          </View>
        </View>

        <View style={styles.timeline}></View>

        <ScrollView
          style={styles.eventsContainer}
          ref={scrollView => this.scrollView = scrollView}
        >
          {/*
            Later on we'll be adding Pull to Refresh behaviour
            -- just like we're doing for the Journal screen
          */}
          {
            events.length > 0
              ? events
              : <View style={styles.noEvents}><Text style={styles.noEventsText}>No events setup in Google Calendar</Text></View>
          }
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
    height: 140,
    zIndex: 6,
    alignItems: 'center',
    justifyContent: 'center',
    position: 'relative',
    paddingTop: 30
  },
  nextEventDate: {
    width: 82,
    height: 110,
    position: 'absolute',
    left: 10,
    bottom: 0,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: variables.colors.status.ok
  },
  dayName: {
    color: 'white',
    fontSize: 12,
    fontWeight: 'bold',
    lineHeight: 18,
    marginBottom: 5
  },
  day: {
    color: 'white',
    flex: 1,
    fontSize: 30,
    fontWeight: 'bold',
    letterSpacing: -1,
    lineHeight: 30
  },
  month: {
    color: 'white',
    fontSize: 18,
    fontWeight: 'bold',
    lineHeight: 18,
    marginTop: 2
  },
  nextEventInformation: {
    flex: 1,
    paddingLeft: 10,
    paddingBottom: 10,
    width: variables.dimensions.width,
    paddingLeft: 112,
    paddingRight: 10,
    paddingBottom: 10,
    flexDirection: 'column',
    justifyContent: 'space-between'
  },
  nextEventInformationHeading: {
    flex: 1,
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  nextText: {
    fontWeight: 'bold',
    color: variables.colors.status.ok
  },
  todayText: {
    color: variables.colors.active
  },
  nextTitle: {
    fontSize: 16,
    fontWeight: 'bold',
    color: variables.colors.gray.darkest
  },
  nextDescription: {
    paddingTop: 4,
  },
  nextMeta: {
    paddingTop: 5,
    flex: 1,
    flexDirection: 'row',
    justifyContent: 'flex-start',
    alignItems: 'center'
  },
  metaText: {
    fontSize: 12,
    lineHeight: 12,
    color: variables.colors.gray.darker
  },
  eventsContainer: {
    flex: 1,
    position: 'relative',
    paddingTop: 20,
    backgroundColor: 'transparent',
    paddingLeft: 10,
    paddingRight: 10,
    zIndex: 5,
  },
  dateContainer: {position: 'relative'},
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
    width: 6,
    height: 6,
    borderRadius: 3,
    backgroundColor: variables.colors.gray.light,
    top: -2,
    left: 88
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
  timeline: {
    width: 2,
    height: variables.dimensions.height,
    backgroundColor: variables.colors.gray.light,
    position: 'absolute',
    left: 90,
    zIndex: 2
  },
  noEvents: {
    backgroundColor: variables.colors.gray.light,
    padding: 10,
    width: variables.dimensions.width-20
  },
  noEventsText: {
    color: variables.colors.gray.dark,
    fontSize: 12,
    fontWeight: 'bold'
  }
});

export default ActivitiesView;

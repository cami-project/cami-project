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

  render() {
    const today = moment();
    const todayMonth = today.format('MMM');
    const firstEventDate = moment.unix(this.props.events.get(0).get('start'));
    const firstEventMonth = firstEventDate ? firstEventDate.format('MMM') : false;

    const events = [];
    let monthKey = firstEventMonth;

    this.props.events.forEach((event, index) => {
      // we want to exclude the 1st event
      // -- we're already displaying it inside the Activities Header
      if (index > 0) {
        // every time a month changes we show a visual separator inside the timeline
        const month = moment.unix(event.get('start')).format('MMM');
        if (month != monthKey) {
          monthKey = month;
          const monthSeparatorText = moment.unix(event.get('start')).format('MMMM');
          events.push(
            <View key={'text' + index} style={[styles.dateContainer, {flex: 1}]}>
              <View style={styles.dateRuler}><View style={styles.dateBullet}/></View>
              <Text style={[styles.date]}>{monthSeparatorText}</Text>
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
            title={event.get('summary')}
            description={event.get('description')}
            location={event.get('location')}
            color={event.get('calendar').get('color').get('background')}
          />
        );
      }
    });

    return (
      <View style={variables.container}>
        <View style={styles.headerContainer}>
          <View style={styles.nextEventDate}>
            <View>
              {
                firstEventDate
                  ? <Text style={styles.day}>{firstEventDate.format('DD').toUpperCase()}</Text>
                  : <Text style={styles.day}>--</Text>
              }
            </View>
            <View>
              {
                firstEventDate
                  ? <Text style={styles.month}>{firstEventDate.format('MMM').toUpperCase()}</Text>
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
              this.props.events.get(0).get('summary')
                ? <Text style={styles.nextTitle}>{this.props.events.get(0).get('summary')}</Text>
                : <Text style={styles.nextTitle}>No pending events</Text>
            }
            {
              this.props.events.get(0).get('description')
                ? <Text style={styles.nextDescription}>{this.props.events.get(0).get('description')}</Text>
                : <Text style={styles.nextDescription}>Add using Google Calendar</Text>
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
                  firstEventDate
                    ? <Text style={styles.metaText}>{firstEventDate.format('hh:mm')}</Text>
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
                  this.props.events.get(0).get('location')
                    ? <Text style={styles.metaText}>{this.props.events.get(0).get('location')}</Text>
                    : <Text style={styles.metaText}>----</Text>
                }
              </View>
            </View>
          </View>
        </View>

        <View style={styles.timeline}></View>

        <ScrollView style={styles.eventsContainer}>
          {/*
            Later on we'll be adding Pull to Refresh behaviour
            -- just like we're doing for the Journal screen
          */}
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
  }
});

export default ActivitiesView;

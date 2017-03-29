import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
} from 'react-native';

import moment from 'moment';
import Icon from 'react-native-vector-icons/FontAwesome';
import icons from 'Cami/src/icons-fa';
import variables from '../../variables/CaregiverGlobalVariables';

const ActivityEntry = React.createClass({
  propTypes: {
    timestamp: PropTypes.number.isRequired,
    title: PropTypes.string.isRequired,
    description: PropTypes.string.isRequired,
    location: PropTypes.string.isRequired,
    color: PropTypes.string.isRequired,
  },

  render() {
    const eventTime = moment.unix(this.props.timestamp);

    return (
      <View style={styles.activityEntry}>
        <View style={styles.dateContainer}>
          <View>
            <Text style={styles.dayName}>{eventTime.format('ddd')}</Text>
          </View>
          <View>
            <Text style={styles.day}>{eventTime.format('DD')}</Text>
          </View>
          <View>
            <Text style={styles.month}>{eventTime.format('MMM').toUpperCase()}</Text>
          </View>
        </View>

        <View style={{
          flex: 7,
          borderLeftWidth: 2,
          borderColor: this.props.color
        }}>
          <View style={styles.textContainer}>
            <View style={{flexWrap: 'wrap'}}>
              <Text style={styles.title}>{this.props.title}</Text>
              {
                this.props.description
                  ? <Text style={styles.description}>{this.props.description}</Text>
                  : ''
              }
            </View>
            <View style={[styles.metaContainer, {flexWrap: 'wrap'}]}>
              <Icon
                name={icons.time}
                size={12}
                color={variables.colors.gray.neutral}
                style={{paddingRight: 5}}
              />
              <View>
                <Text style={styles.metaText}>{eventTime.format('HH:mm')}</Text>
              </View>
              <Icon
                name={icons.location}
                size={12}
                color={variables.colors.gray.neutral}
                style={{paddingRight: 5, marginLeft: 10}}
              />
              <View>
                <Text style={styles.metaText}>{this.props.location}</Text>
              </View>
            </View>
          </View>
        </View>
      </View>
    );
  }
});

const styles = StyleSheet.create({
  activityEntry: {
    backgroundColor: 'white',
    marginBottom: 20,
    flexDirection: 'row',
    justifyContent: 'flex-start',
    alignItems: 'center',
  },
  dateContainer: {
    width: 80,
    paddingTop: 20,
    paddingBottom: 20,
    paddingLeft: 20,
    paddingRight: 20,
    flexDirection: 'column',
    alignSelf: 'center',
    alignItems: 'center',
    zIndex: 7,
  },
  dayName: {
    fontSize: 12,
    fontWeight: 'bold',
    color: variables.colors.gray.dark,
    marginBottom: 5
  },
  day: {
    fontSize: 26,
    fontWeight: 'bold',
    lineHeight: 26,
    letterSpacing: -1,
    color: variables.colors.gray.dark
  },
  month: {
    fontSize: 14,
    fontWeight: 'bold',
    color: variables.colors.gray.dark
  },
  textContainer: {
    flex: 1,
    flexWrap: 'wrap',
    justifyContent: 'center',
    flexDirection: 'column',
    paddingTop: 20,
    paddingBottom: 20,
    paddingLeft: 20,
    paddingRight: 20,
  },
  title: {
    color: variables.colors.gray.darker,
    fontWeight: 'bold',
    fontSize: 12
  },
  description: {
    paddingTop: 5,
    fontSize: 12,
    color: variables.colors.gray.dark
  },
  metaContainer: {
    paddingTop: 10,
    marginTop: 10,
    borderTopWidth: 1,
    borderStyle: 'solid',
    borderTopColor: variables.colors.gray.light,
    flex: 1,
    flexDirection: 'row',
    justifyContent: 'flex-start',
    alignItems: 'center'
  },
  metaText: {
    fontSize: 12,
    lineHeight: 12,
    color: variables.colors.gray.darker,
  },
});

export default ActivityEntry;

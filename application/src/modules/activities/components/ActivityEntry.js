import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
} from 'react-native';

import moment from 'moment';
import Color from 'color';
import Icon from 'react-native-vector-icons/FontAwesome';
import icons from 'Cami/src/icons-fa';
import variables from '../../variables/CaregiverGlobalVariables';

const ActivityEntry = React.createClass({
  propTypes: {
    timestamp: PropTypes.number.isRequired,
    title: PropTypes.string,
    description: PropTypes.string,
    location: PropTypes.string,
    color: PropTypes.string,
    archived: PropTypes.bool.isRequired,
    today: PropTypes.bool.isRequired,
  },

  render() {
    const eventTime = moment.unix(this.props.timestamp).utc();

    return (
      <View style={[
        styles.activityEntry,
        {
          opacity: this.props.archived ? 0.5 : 1,
          backgroundColor: !this.props.today ? 'white' : Color(variables.colors.status.ok).clearer(.25).rgbaString()
        }
      ]}>
        <View style={styles.dateContainer}>
          <View>
            <Text style={[styles.dayName, {color: !this.props.today ? variables.colors.gray.dark : 'white'}]}>{eventTime.format('ddd')}</Text>
          </View>
          <View>
            <Text style={[styles.day, {color: !this.props.today ? variables.colors.gray.dark : 'white'}]}>{eventTime.format('DD')}</Text>
          </View>
          <View>
            <Text style={[styles.month, {color: !this.props.today ? variables.colors.gray.dark : 'white'}]}>{eventTime.format('MMM').toUpperCase()}</Text>
          </View>
        </View>

        <View style={{
          flex: 7,
          borderLeftWidth: 2,
          borderColor: !this.props.archived ? this.props.color : variables.colors.gray.neutral,
          backgroundColor: 'white'
        }}>
          <View style={styles.textContainer}>
            <View style={{flexWrap: 'wrap'}}>
              <Text style={styles.title}>{this.props.title}</Text>
              {
                this.props.description !== 'No description set'
                  ? <Text style={styles.description}>{this.props.description}</Text>
                  : <Text style={[styles.description, {color: variables.colors.gray.neutral}]}>{this.props.description}</Text>
              }
            </View>
            <View style={[styles.metaContainer, {flexDirection: 'row', flex: 1}]}>
              <View style={{flexDirection: 'row'}}>
                <Icon
                  name={icons.time}
                  size={12}
                  color={variables.colors.gray.neutral}
                  style={{paddingRight: 5}}
                />
                <Text style={styles.metaText}>{eventTime.format('HH:mm')}</Text>
              </View>
              <View style={{flexDirection: 'row'}}>
                <Icon
                  name={icons.location}
                  size={12}
                  color={variables.colors.gray.neutral}
                  style={{paddingRight: 5, marginLeft: 10}}
                />
                {
                  this.props.location !== 'No location set'
                    ? <Text numberOfLines={1} ellipsizeMode='clip' style={[styles.metaText, {width: 170}]}>{this.props.location}</Text>
                    : <Text style={[styles.metaText, {color: variables.colors.gray.neutral}]}>{this.props.location}</Text>
                }
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
  archived: {
    opacity: 0.75
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

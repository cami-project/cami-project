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

  render() {
    const time = moment(new Date(this.props.timestamp * 1000)).format('HH mm');

    return (
      <View style={{backgroundColor: 'white', flexDirection: 'row'}}>
        <View>
          <Icon name={icons[this.props.type]} size={20} color={variables.colors.status[this.props.status]}/>
        </View>
        <View>
          <Text>{time}</Text>
        </View>
        <View style={{flex: 1}}>
          <Text>{this.props.title}</Text>
        </View>
      </View>
    );
  }
});

export default JournalEntry;

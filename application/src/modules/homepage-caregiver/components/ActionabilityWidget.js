import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
  TouchableOpacity,
  Animated
} from 'react-native';

import moment from 'moment';
import Icon from 'react-native-vector-icons/FontAwesome';
import icons from 'Cami/src/icons-fa';
import variables from '../../variables/CaregiverGlobalVariables';
import Sound from 'react-native-sound';

import * as HomepageState from '../HomepageState';

const dummySound = new Sound('sounds/just-like-that.mp3', Sound.MAIN_BUNDLE, (error) => {
  if (error) {
    console.log('failed to load the sound', error);
  }
});

const ActionabilityWidget = React.createClass({
  propTypes: {
    name: PropTypes.string.isRequired,
    icon: PropTypes.string.isRequired,
    timestamp: PropTypes.number.isRequired,
    message: PropTypes.string.isRequired,
    description: PropTypes.string.isRequired,
    dispatch: PropTypes.func.isRequired
  },

  getInitialState() {
    return {
      backgroundColor: new Animated.Value(0)
    };
  },

  runBackgroundColorAnimation() {
    const me = this;
    Animated.sequence([
      Animated.timing(this.state.backgroundColor, {
        toValue: 10,
        duration: 1500,
        delay: 3000
      }),
      Animated.timing(this.state.backgroundColor, {
        toValue: 0,
        duration: 1500
      })
    ]).start((data) => {
      me.runBackgroundColorAnimation();
    });
  },

  componentDidMount() {
    this.runBackgroundColorAnimation();
  },

  call911() {
    dummySound.setVolume(1.0).play();
  },

  callElderly() {
    dummySound.setVolume(1.0).play();
  },

  cancel() {
    dummySound.setVolume(1.0).play();
    this.props.dispatch(HomepageState.hideActionability());
  },

  render() {
    const backgroundColor = this.state.backgroundColor.interpolate({
        inputRange: [0, 10],
        outputRange: ['#ECCDCD', 'rgba(0, 0, 0, 0)']
    });
    const time = moment(new Date(this.props.timestamp * 1000)).format('HH:mm');

    return (
      <Animated.View style={{flex: 1, backgroundColor: backgroundColor}}>
        <View style={{flexDirection: 'row', flex: 2}}>
          <View style={{flex: 1, alignItems: 'center', justifyContent: 'center'}}>
            <Icon name={this.props.icon} size={34} color={variables.colors.status.alert}/>
            <Text style={[{fontSize: 14, fontWeight: 'bold'}]}>{time}</Text>
          </View>
          <View style={{flex: 3, alignItems: 'flex-start', justifyContent: 'center'}}>
            <Text style={[{fontSize: 14}]}>{this.props.message}</Text>
            <Text style={[{fontSize: 14, marginTop: 10, paddingRight: 20}]}>{this.props.description}</Text>
          </View>
        </View>
        <View style={{flexDirection: 'row', flex: 1}}>
          <TouchableOpacity
            style={{flex: 1, alignItems: 'center', justifyContent: 'center', backgroundColor: variables.colors.status.alert}}
            onPress={this.call911}
          >
            <Icon name={icons.plus} size={16} color="white"/>
            <Text style={[{fontSize: 14, color: "white", fontWeight: 'bold', paddingTop: 5}]}>Call 911</Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={{flex: 2, alignItems: 'center', justifyContent: 'center', backgroundColor: variables.colors.status.ok}}
            onPress={this.callElderly}
          >
            <Icon name={icons.phone} size={16} color="white"/>
            <Text style={[{fontSize: 14, color: "white", fontWeight: 'bold', paddingTop: 5}]}>Call {this.props.name}</Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={{flex: 1, alignItems: 'center', justifyContent: 'center', backgroundColor: variables.colors.gray.neutral}}
            onPress={this.cancel}
          >
            <Icon name={icons.cancel} size={16} color="white"/>
            <Text style={[{fontSize: 14, color: "white", fontWeight: 'bold', paddingTop: 5}]}>Dismiss</Text>
          </TouchableOpacity>
        </View>
      </Animated.View>
    );
  }
});

export default ActionabilityWidget;

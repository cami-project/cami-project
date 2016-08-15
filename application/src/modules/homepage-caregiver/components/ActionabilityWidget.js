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

const ActionabilityWidget = React.createClass({
  propTypes: {
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

  render() {
    const backgroundColor = this.state.backgroundColor.interpolate({
        inputRange: [0, 10],
        outputRange: ['#ECCDCD', 'rgba(0, 0, 0, 0)']
    });

    return (
      <Animated.View style={{flex: 1, backgroundColor: backgroundColor}}>
        <View style={{flexDirection: 'row', flex: 2}}>
          <View style={{flex: 1, alignItems: 'center', justifyContent: 'center'}}>
            <Icon name={icons.heart} size={34} color={variables.colors.status.alert}/>
            <Text style={[{fontSize: 14, fontWeight: 'bold'}]}>15:41</Text>
          </View>
          <View style={{flex: 3, alignItems: 'flex-start', justifyContent: 'center'}}>
            <Text style={[{fontSize: 14}]}>Jim's heart rate has increased drastically!</Text>
            <Text style={[{fontSize: 14, marginTop: 10}]}>He has been reminded 3 times to take his Accupril, but did not respond.</Text>
          </View>
        </View>
        <View style={{flexDirection: 'row', flex: 1}}>
          <TouchableOpacity style={{flex: 1, alignItems: 'center', justifyContent: 'center', backgroundColor: variables.colors.status.alert}}>
            <Icon name={icons.plus} size={16} color="white"/>
            <Text style={[{fontSize: 14, color: "white", fontWeight: 'bold'}]}>Call 911</Text>
          </TouchableOpacity>
          <TouchableOpacity style={{flex: 2, alignItems: 'center', justifyContent: 'center', backgroundColor: variables.colors.status.ok}}>
            <Icon name={icons.phone} size={16} color="white"/>
            <Text style={[{fontSize: 14, color: "white", fontWeight: 'bold'}]}>Call Jim</Text>
          </TouchableOpacity>
          <TouchableOpacity style={{flex: 1, alignItems: 'center', justifyContent: 'center', backgroundColor: variables.colors.gray.neutral}}>
            <Icon name={icons.cancel} size={16} color="white"/>
            <Text style={[{fontSize: 14, color: "white", fontWeight: 'bold'}]}>Cancel</Text>
          </TouchableOpacity>
        </View>
      </Animated.View>
    );
  }
});

export default ActionabilityWidget;

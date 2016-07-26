import {Map} from 'immutable'
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

import StatusChart from './components/StatusChart';

const HomepageView = React.createClass({
  propTypes: {
    status: PropTypes.object.isRequired,
    username: PropTypes.string.isRequired
  },

  render() {
    const heartRateData = this.props.status.get('heart').get('rate');
    const weightData = this.props.status.get('weigth').get('amount');

    return (
      <View style={styles.container}>

        <View style={styles.iconContainer}>
          <View style={styles.outerRing}>
            <Image style={styles.iconRing} source={require('../../../images/old-man.png')}/>
          </View>
          <Text style={[styles.mainText, {fontWeight: 'bold'}]}>
            {this.props.username}'s doing fine
          </Text>
        </View>

        <View style={styles.mainContainer}>
          <View style={{flexDirection: 'row'}}>
            <StatusChart
              data={heartRateData}
              text="Heart rate"
              image={require('../../../images/heart-ok.png')}
              unit="bpm"/>

            <StatusChart
              data={weightData}
              text="Weight"
              image={require('../../../images/weight-warning.png')}
              unit="kg"/>
          </View>
        </View>
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
    paddingTop: 20
  },
  outerRing: {
    borderWidth: 2,
    borderRadius: 72,
    width: 140,
    height: 140,
    borderColor: Color('white').clearer(.75).rgbaString(),
    marginBottom: -70,
    justifyContent: 'center'
  },
  iconRing: {
    borderWidth: 0,
    borderRadius: 60,
    width: 120,
    height: 120,
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

export default HomepageView;

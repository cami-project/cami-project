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
import Chart from 'react-native-chart';

// import React, { StyleSheet, View, Component } from 'react-native';

var Color = require("color");

const chartStyles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    // backgroundColor: 'white',
  },
  chart: {
    // flex: 1,
    width: 100,
    height: 50,
  },
});

const data = [
  [0, 1],
  [1, 3],
  [3, 7],
  [4, 9],
];

const HomepageView = React.createClass({
  propTypes: {
    status: PropTypes.object.isRequired,
    username: PropTypes.string.isRequired
  },

  render() {
    const rate = this.props.status.get('heart').get('rate');
    const heartRateData = [];
    rate.forEach((value, index) => heartRateData.push([index, value]));
    const bpm = rate.get(rate.size - 1);

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
          {/* Heart rate */}
          <View>
            <Text style={styles.textValue}>
              {bpm} <Text style={styles.textUnit}>bpm</Text>
            </Text>
            <Text style={styles.textDescription}>Heart Rate</Text>
            <View style={chartStyles.container}>
              <Chart
                style={chartStyles.chart}
                data={heartRateData}
                type="line"
                showGrid={false}
                showAxis={false}
              />
            </View>
          </View>
        </View>
      </View>
    );
  }
});

const buttonCircle = {
  borderWidth: 0,
  borderRadius: 45,
  width: 90,
  height: 90
};

let {height, width} = Dimensions.get('window');

const color = {
  developing: '#a7b50a'
}

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
  textContainer: {
    paddingTop: 40,
    width: width*.8,
    justifyContent: 'center'
  },
  mainText: {
    fontSize: 26,
    color: 'white',
    lineHeight: 1.3*26
  },
  textValue: {
    fontSize: 26,
    color: 'black',
    lineHeight: 1.3*26
  },
  textUnit: {
    fontSize: 16,
    color: 'black',
    lineHeight: 1.3*26
  },
  textDescription: {
    fontSize: 18,
    color: 'black',
    lineHeight: 1.3*26
  },
  text: {
    fontSize: 26,
    color: '#158A12',
    lineHeight: 1.3*26
  },
  buttonContainer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    width: width*.8,
    position: 'absolute',
    bottom: width*.1,
    left: width*.1
  },
  button: {
    ...buttonCircle,
    alignSelf: 'center',
    justifyContent: 'center',
    shadowRadius: 40,
    shadowOffset: {width: 0, height: 0},
    shadowOpacity: 0.4,
    shadowColor: Color(color.developing).darken(.6).hexString()
  },
  buttonText: {
    backgroundColor: 'transparent',
    textAlign: 'center',
    alignSelf: 'center',
    fontSize: 22,
    fontWeight: 'bold'
  },
  buttonConfirm: {
    backgroundColor: 'white',
  },
  buttonPanic: {
    backgroundColor: '#C95F5F'
  }
});

export default HomepageView;

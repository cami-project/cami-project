import {List} from 'immutable'
import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
  Image
} from 'react-native';
import Chart from 'react-native-chart';
import {LineChart} from 'react-native-ios-charts';
import moment from 'moment';

import {images} from 'Cami/src/images';

const chartStyles = StyleSheet.create({
  container: {
    flex: 1
  },
  chartContainer: {
    flex: 1,
    backgroundColor: 'white',
    padding: 10
  },
  chart: {
    flex: 1,
    height: 150,
    paddingBottom: 10
  },
  value: {
    fontSize: 26,
    color: 'black',
    lineHeight: 1.3*26,
    textAlign: 'right'
  },
  unit: {
    fontSize: 16,
    color: 'black',
    lineHeight: 1.3*26
  },
  description: {
    fontSize: 18,
    color: 'black',
    lineHeight: 1.3*26
  }
});

const config = {
  dataSets: [{
    values: [-1, 1, -1, 1, -1, 1],
    drawValues: false,
    colors: ['rgb(199, 255, 140)'],
    drawCubic: false,
    drawCircles: false,
    lineWidth: 2
  }],
  backgroundColor: 'red',
  labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
  // minOffset: 20,
  scaleYEnabled: false,
  legend: {
    enabled: false,
    position: 'leftOfChart',
  },
  xAxis: {
    axisLineWidth: 0,
    drawLabels: true,
    position: 'bottom',
    drawGridLines: false
  },
  leftAxis: {
    enabled: false,
  },
  rightAxis: {
    enabled: false
  }
};

const StatusEntry = React.createClass({
  propTypes: {
    type: PropTypes.string.isRequired,
    status: PropTypes.string.isRequired,
    data: PropTypes.instanceOf(List).isRequired,
  },

  formatTimestamp(timestamp) {
    return moment(new Date(timestamp * 1000)).format('HH');
  },

  render() {
    const xValues = [];
    const yValues = [];
    this.props.data.forEach(item => {
      xValues.push(this.formatTimestamp(item.get('timestamp')));
      yValues.push(item.get('value'));
    });
    const lastValue = this.props.data.get(this.props.data.size - 1).get('value');
    const chartConfig = {...config, labels: xValues}
    chartConfig.dataSets[0].values = yValues;

    return (
      <View style={chartStyles.container}>
        <View style={{flexDirection: 'row'}}>
          <View style={{justifyContent: 'center', alignItems: 'center'}}>
            <Image source={images[this.props.type][this.props.status]}/>
          </View>
          <View>
            <Text>Heart rate</Text>
          </View>
          <View style={{flex: 1}}>
            <Text style={chartStyles.value}>
              {lastValue} <Text style={chartStyles.unit}>bpm</Text>
            </Text>
          </View>
        </View>
        <View style={chartStyles.chartContainer}>
          <LineChart config={chartConfig} style={chartStyles.chart}/>
        </View>
      </View>
    );
  }
});

export default StatusEntry;

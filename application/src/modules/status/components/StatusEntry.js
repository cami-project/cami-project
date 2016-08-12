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
    height: 80,
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
    drawValues: false,
    colors: ['rgb(199, 255, 140)'],
    drawCubic: false,
    drawCircles: true,
    lineWidth: 2
  }],
  backgroundColor: 'blue',
  // minOffset: 20,
  scaleYEnabled: false,
  legend: {
    enabled: false,
    position: 'pieChartCenter',
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
    title: PropTypes.string.isRequired,
    type: PropTypes.string.isRequired,
    data: PropTypes.instanceOf(List).isRequired,
    threshold: PropTypes.oneOfType([PropTypes.number, PropTypes.string]).isRequired,
    units: PropTypes.string,
    timeUnits: PropTypes.string.isRequired,
    formatValue: PropTypes.func,
    timestampFormat: PropTypes.string.isRequired
  },

  getDefaultProps() {
    return {
      timestampFormat: 'HH'
    };
  },

  formatTimestamp(timestamp) {
    return moment(new Date(timestamp * 1000)).format(this.props.timestampFormat);
  },

  formatStatus(status) {
    return {
      'ok': 'green',
      'warning': 'yellow',
      'alert': 'red'
    }[status];
  },

  formatValue(status, value, threshold) {
    return {
      'ok': 0,
      'warning': value > threshold ? 1 : -1,
      'alert': value > threshold ? 1 : -1
    }[status];
  },

  render() {
    const formatValue = this.props.formatValue ? this.props.formatValue : this.formatValue;
    const chartConfig = {...config, labels: []};
    const dataSet = chartConfig.dataSets[0];
    dataSet.values = [];
    dataSet.circleColors = [];
    this.props.data.forEach(item => {
      chartConfig.labels.push(this.formatTimestamp(item.get('timestamp')));
      dataSet.values.push(formatValue(item.get('status'), item.get('value'), this.props.threshold));
      dataSet.circleColors.push(this.formatStatus(item.get('status')));
    });
    const lastValue = this.props.data.get(this.props.data.size - 1).get('value');
    const lastStatus = this.props.data.get(this.props.data.size - 1).get('status');

    return (
      <View style={chartStyles.container}>
        <View style={{flexDirection: 'row'}}>
          <View style={{justifyContent: 'center', alignItems: 'center'}}>
            <Image source={images[this.props.type][lastStatus]}/>
          </View>
          <View>
            <Text>{this.props.title}</Text>
          </View>
          <View style={{flex: 1}}>
            <Text style={chartStyles.value}>
              {lastValue} <Text style={chartStyles.unit}>{this.props.units}</Text>
            </Text>
          </View>
        </View>
        <View style={{flexDirection: 'row'}}>
          <View style={{justifyContent: 'flex-end', alignItems: 'flex-end'}}>
            <Text>{this.props.timeUnits}</Text>
          </View>
          <View style={chartStyles.chartContainer}>
            <LineChart config={chartConfig} style={chartStyles.chart}/>
          </View>
        </View>
      </View>
    );
  }
});

export default StatusEntry;

import {List} from 'immutable'
import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
  Image
} from 'react-native';
import Chart from 'react-native-chart';
import moment from 'moment';

import {images} from 'Cami/src/images';

const chartStyles = StyleSheet.create({
  container: {
    flex: 1
  },
  chartContainer: {
    // flex: 1,
    backgroundColor: 'white',
    padding: 10
  },
  chart: {
    height: 100,
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
    const arrayData = [];
    this.props.data.forEach(item =>
      arrayData.push([item.get('timestamp'), item.get('value')])
    );
    const lastValue = this.props.data.get(this.props.data.size - 1).get('value');

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
        <View>
          <View style={chartStyles.chartContainer}>
            <Chart
              style={chartStyles.chart}
              data={arrayData}
              type="line"
              showGrid={false}
              showAxis={true}
              showDataPoint={true}
              xAxisTransform={this.formatTimestamp}
            />
          </View>
        </View>
      </View>
    );
  }
});

export default StatusEntry;

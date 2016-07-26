import {List} from 'immutable'
import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
  Image
} from 'react-native';
import Chart from 'react-native-chart';

const chartStyles = StyleSheet.create({
  container: {
    flex: 1
  },
  chartContainer: {
    // flex: 1,
    // backgroundColor: 'white',
  },
  chart: {
    height: 50,
  },
  value: {
    fontSize: 26,
    color: 'black',
    lineHeight: 1.3*26
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

const StatusChart = React.createClass({
  propTypes: {
    image: PropTypes.number.isRequired,
    data: PropTypes.instanceOf(List).isRequired,
    unit: PropTypes.string.isRequired,
    text: PropTypes.string.isRequired
  },

  render() {
    const arrayData = [];
    this.props.data.forEach((value, index) => arrayData.push([index, value]));
    const currentValue = this.props.data.get(this.props.data.size - 1);

    return (
      <View style={chartStyles.container}>
        <View style={{flexDirection: 'row'}}>
          <View style={{justifyContent: 'center', alignItems: 'center'}}>
            <Image source={this.props.image}/>
          </View>
          <View>
            <Text style={chartStyles.value}>
              {currentValue} <Text style={chartStyles.unit}>{this.props.unit}</Text>
            </Text>
            <Text style={chartStyles.description}>{this.props.text}</Text>
          </View>
        </View>
        <View style={chartStyles.chartContainer}>
          <Chart
            style={chartStyles.chart}
            data={arrayData}
            type="line"
            showGrid={false}
            showAxis={false}
          />
        </View>
      </View>
    );
  }
});

export default StatusChart;

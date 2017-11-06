import {List} from 'immutable'
import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View
} from 'react-native';
import Chart from 'react-native-chart';
import Icon from 'react-native-vector-icons/FontAwesome';
import variables from '../../variables/CaregiverGlobalVariables';

const chartStyles = StyleSheet.create({
  container: {
    flex: 1,
    padding: 10,
    borderBottomWidth: 1,
    borderRightWidth: 1,
    borderColor: variables.colors.gray.light,
    borderStyle: 'solid'
  },
  chartContainer: {
    paddingTop: 15
  },
  chart: {
    height: 25,
  },
  iconContainer: {
    width: 40,
    justifyContent: 'flex-start',
    alignSelf: 'flex-start',
    alignItems: 'flex-start',
    height: 40
  },
  infoContainer: {
    paddingTop: 2,
    paddingLeft: 10
  },
  value: {
    fontSize: 24,
    color: variables.colors.gray.darker,
    lineHeight: 24
  },
  unit: {
    fontSize: 12,
    fontWeight: 'bold',
    color: variables.colors.gray.neutral,
    lineHeight: 24
  },
  description: {
    fontSize: 12,
    color: variables.colors.gray.darker,
    lineHeight: 12
  }
});

const StatusChart = React.createClass({
  propTypes: {
    icon: PropTypes.string.isRequired,
    data: PropTypes.instanceOf(List).isRequired,
    unit: PropTypes.string.isRequired,
    text: PropTypes.string.isRequired,
    status: PropTypes.string.isRequired,
    decimals: PropTypes.number
  },

  render() {
    const arrayData = [];
    this.props.data.forEach((value, index) => arrayData.push([index, value]));
    const currentValue = this.props.data.get(this.props.data.size - 1);

    return (
      <View style={chartStyles.container}>
        <View style={{flexDirection: 'row'}}>
          <View style={[chartStyles.iconContainer, {justifyContent: 'center'}]}>
            <Icon name={this.props.icon} size={34} color={variables.colors.status[this.props.status]}/>
          </View>
          <View style={chartStyles.infoContainer}>
            {
              this.props.data.size > 0 ?
                <Text style={chartStyles.value}>
                  {
                    this.props.decimals > 0
                      ? currentValue.toFixed(this.props.decimals)
                      : currentValue
                  } <Text style={chartStyles.unit}>{this.props.unit}</Text>
                </Text>
                :
                <Text style={chartStyles.value}>
                  <Text>N/A</Text> <Text style={chartStyles.unit}>{this.props.unit}</Text>
                </Text>
            }
            <Text style={chartStyles.description}>{this.props.text}</Text>
          </View>
        </View>
        <View style={chartStyles.chartContainer}>
          {
            this.props.data.size > 0 ?
              <Chart
                style={chartStyles.chart}
                data={arrayData}
                type="line"
                showGrid={false}
                showAxis={false}
                color="#999999"
                lineWidth={2}
              />
              : <Text style={chartStyles.description}>No weight information</Text>
          }
        </View>
      </View>
    );
  }
});

export default StatusChart;

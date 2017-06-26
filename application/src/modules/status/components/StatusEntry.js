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

import Icon from 'react-native-vector-icons/FontAwesome';
import icons from 'Cami/src/icons-fa';
import moment from 'moment';
import variables from 'Cami/src/modules/variables/CaregiverGlobalVariables';

const config = {
  dataSets: [{
    drawValues: false,
    colors: [variables.colors.gray.neutral],
    drawCubic: false,
    drawCircles: true,
    circleRadius: 4,
    lineWidth: 2,
    drawMarkers: false,
    drawLimitLinesBehindData: false,
    iserInteractionEnabled: false,
    highlightPerTap: false,
    highlightValues: false,
    drawHorizontalHighlightIndicator: false,
    drawVerticalHighlightIndicator: false
  }],
  backgroundColor: '#ffffff',
  // minOffset: 20,
  scaleYEnabled: false,
  legend: {
    enabled: false,
    position: 'pieChartCenter',
    formSize: 0
  },
  xAxis: {
    axisLineWidth: 0,
    drawLabels: true,
    position: 'bottom',
    drawGridLines: false,
    drawCubic: false,
    drawLimitLinesBehindData: false,
    textColor: variables.colors.gray.dark,
    textSize: 11
  },
  leftAxis: {
    enabled: false,
    drawCubic: false,
  },
  rightAxis: {
    enabled: false,
    drawCubic: false,
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
    timestampFormat: PropTypes.string.isRequired,
    total: PropTypes.instanceOf(List)
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
      'ok': variables.colors.gray.neutral,
      'warning': variables.colors.status.warning,
      'alert': variables.colors.status.alert
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

    // steps json comes w/ start/end timestamps instead of just one
    const timestampKey = this.props.type != 'steps' ? 'timestamp' : 'end_timestamp';

    dataSet.values = [];
    dataSet.circleColors = [];
    this.props.data.forEach(item => {
      chartConfig.labels.push(this.formatTimestamp(item.get(timestampKey)));

      var value = formatValue(item.get('status'), item.get('value'), this.props.threshold);
      if(this.props.type == 'weight' || this.props.type == 'steps') {
        value = item.get('value');
      }
      dataSet.values.push(value);
      dataSet.circleColors.push(this.formatStatus(item.get('status')));
    });

    console.log('DATA SET -------------');
    console.log(dataSet);

    // for the majority of the status widgets we get the most recent value
    var lastValue = this.props.data.size > 0 ? this.props.data.get(this.props.data.size - 1).get('value') : 0;

    // for steps we're intersed in the total amount of steps
    if(this.props.type == 'steps') {
      lastValue = this.props.total.size > 0 ? this.props.total.get(this.props.total.size - 1) : 0;
    }

    const lastStatus = this.props.data.size > 0 ? this.props.data.get(this.props.data.size - 1).get('status') : "ok";

    return (
      <View style={styles.container}>
        <View style={styles.header}>
          <View style={styles.information}>
            <View style={{width: 30, justifyContent: 'flex-start'}}>
              <Icon name={icons[this.props.type]} size={20} color={variables.colors.status[lastStatus]} style={{alignSelf: 'center'}}/>
            </View>
            <Text style={styles.title}>{this.props.title}</Text>
          </View>
          <View style={styles.values}>
            <Text style={styles.unit}>{this.props.units}</Text>
            <Text style={styles.value}>{lastValue}</Text>
          </View>
        </View>
        <View style={styles.content}>
          <View style={styles.timeUnitContainer}>
            <Text style={styles.timeUnit}>{this.props.timeUnits}</Text>
          </View>
          <View style={styles.chartContainer}>
            <LineChart config={chartConfig} style={styles.chart}/>
          </View>
        </View>
      </View>
    );
  }
});

const styles = StyleSheet.create({
  container: {
    flex: 1,
    marginTop: 10,
    marginBottom: 10,
    backgroundColor: 'white'
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingTop: 10,
    paddingRight: 10,
    paddingLeft: 10
  },
  content: {
    flexDirection: 'row'
  },
  information: {
    flexDirection: 'row',
    alignItems: 'flex-start'
  },
  values: {
    flexDirection: 'row',
    alignItems: 'flex-end'
  },
  title: {
    fontSize: 14,
    fontWeight: 'bold',
    paddingLeft: 10,
    color: variables.colors.gray.darkest
  },
  unit: {
    fontSize: 12,
    color: variables.colors.gray.dark,
    paddingRight: 5
  },
  value: {
    fontSize: 18,
    color: variables.colors.gray.darkest
  },
  timeUnitContainer: {
    justifyContent: 'flex-end',
    alignItems: 'flex-start',
    paddingLeft: 10,
    width: 40,
    paddingBottom: 10
  },
  timeUnit: {
    fontSize: 12,
    fontWeight: 'bold',
    color: variables.colors.gray.neutral,
  },
  chartContainer: {
    flex: 1,
    backgroundColor: 'white',
    padding: 10
  },
  chart: {
    flex: 1,
    height: 80,
    paddingBottom: 30,
    backgroundColor: 'white'
  },
  description: {
    fontSize: 18,
    color: 'black',
    lineHeight: 1.3*26
  }
});

export default StatusEntry;

import {Map} from 'immutable'
import React, {PropTypes} from 'react';

import StatusEntry from './StatusEntry';

const BloodPressureEntry = React.createClass({
  propTypes: {
    aggregated: PropTypes.instanceOf(Map).isRequired,
    systolic: PropTypes.instanceOf(Map).isRequired,
    diastolic: PropTypes.instanceOf(Map).isRequired,
    aggregated: PropTypes.instanceOf(Map).isRequired,
    pulse: PropTypes.instanceOf(Map).isRequired
  },

  formatValue(status, value, threshold) {
    thresholdMetrics = threshold.split('/');
    thresholdSystolic = parseInt(thresholdMetrics[0]);
    thresholdDiastolic = parseInt(thresholdMetrics[1]);
    valueMetrics = value.split('/');
    valueSystolic = parseInt(valueMetrics[0]);
    valueDiastolic = parseInt(valueMetrics[1]);
    return {
      'ok': 0,
      'warning': valueSystolic > thresholdSystolic || valueDiastolic > thresholdDiastolic ? 1 : -1,
      'alert': valueSystolic > thresholdSystolic || valueDiastolic > thresholdDiastolic ? 1 : -1,
    }[status];
  },

  render() {
    return (
      <StatusEntry
        title="Blood Pressure"
        type="blood"
        data={this.props.aggregated.get('data')}
        threshold={this.props.aggregated.get('threshold')}
        units="mmHg"
        timeUnits="Day"
        timestampFormat="DD"
        formatValue={this.formatValue}
      />
    );
  }
});

export default BloodPressureEntry;

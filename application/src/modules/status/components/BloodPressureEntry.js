import {Map} from 'immutable'
import React, {PropTypes} from 'react';

import StatusEntry from './StatusEntry';

const BloodPressureEntry = React.createClass({
  propTypes: {
    statusItem: PropTypes.instanceOf(Map).isRequired
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
        data={this.props.statusItem.get('data')}
        threshold={this.props.statusItem.get('threshold')}
        units="mmHg"
        timeUnits="Hrs"
        formatValue={this.formatValue}
      />
    );
  }
});

export default BloodPressureEntry;

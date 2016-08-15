import {Map} from 'immutable'
import React, {PropTypes} from 'react';

import StatusEntry from './StatusEntry';

const WeightEntry = React.createClass({
  propTypes: {
    statusItem: PropTypes.instanceOf(Map).isRequired
  },

  render() {
    return (
      <StatusEntry
        title="Weight"
        type="weight"
        data={this.props.statusItem.get('data')}
        threshold={this.props.statusItem.get('threshold')}
        units="kg"
        timeUnits="Day"
        timestampFormat="DD"
      />
    );
  }
});

export default WeightEntry;

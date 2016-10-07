import {Map} from 'immutable'
import React, {PropTypes} from 'react';

import StatusEntry from './StatusEntry';

const WeightEntry = React.createClass({
  propTypes: {
    weight: PropTypes.instanceOf(Map).isRequired
  },

  render() {
    return (
      <StatusEntry
        title="Weight"
        type="weight"
        data={this.props.weight.get('data')}
        threshold={this.props.weight.get('threshold')}
        units="kg"
        timeUnits="Day"
        timestampFormat="DD"
      />
    );
  }
});

export default WeightEntry;

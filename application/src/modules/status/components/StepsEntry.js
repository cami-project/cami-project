import {Map} from 'immutable'
import React, {PropTypes} from 'react';

import StatusEntry from './StatusEntry';

const StepsEntry = React.createClass({
  propTypes: {
    steps: PropTypes.instanceOf(Map).isRequired
  },

  render() {
    return (
      <StatusEntry
        title="Steps"
        type="steps"
        data={this.props.steps.get('data')}
        threshold={this.props.steps.get('threshold') !== undefined ? this.props.steps.get('threshold') : 0}
        units=""
        timeUnits="Day"
        timestampFormat="DD"
        total={this.props.steps.get('amount')}
      />
    );
  }
});

export default StepsEntry;

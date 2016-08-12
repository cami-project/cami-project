import {Map} from 'immutable'
import React, {PropTypes} from 'react';

import StatusEntry from './StatusEntry';

const StepsEntry = React.createClass({
  propTypes: {
    statusItem: PropTypes.instanceOf(Map).isRequired
  },

  render() {
    return (
      <StatusEntry
        title="Steps"
        type="steps"
        data={this.props.statusItem.get('data')}
        threshold={this.props.statusItem.get('threshold')}
        units=""
        timeUnits="Day"
        timestampFormat="DD"
      />
    );
  }
});

export default StepsEntry;

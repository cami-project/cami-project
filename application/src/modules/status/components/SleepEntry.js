import {Map} from 'immutable'
import React, {PropTypes} from 'react';

import StatusEntry from './StatusEntry';

const SleepEntry = React.createClass({
  propTypes: {
    statusItem: PropTypes.instanceOf(Map).isRequired
  },

  render() {
    return (
      <StatusEntry
        title="Sleep"
        type="sleep"
        data={this.props.statusItem.get('data')}
        threshold={this.props.statusItem.get('threshold')}
        units="hrs"
        timeUnits="Day"
        timestampFormat="DD"
      />
    );
  }
});

export default SleepEntry;

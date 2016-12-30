import {Map} from 'immutable'
import React, {PropTypes} from 'react';

import StatusEntry from './StatusEntry';

const HeartRateEntry = React.createClass({
  propTypes: {
    heart: PropTypes.instanceOf(Map).isRequired
  },

  render() {
    return (
      <StatusEntry
        title="Heart rate"
        type="heart"
        data={this.props.heart.get('data')}
        threshold={this.props.heart.get('threshold')}
        units="bpm"
        timeUnits="Hrs"
      />
    );
  }
});

export default HeartRateEntry;

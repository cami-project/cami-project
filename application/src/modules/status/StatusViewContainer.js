import {connect} from 'react-redux';

import StatusView from './StatusView';

export default connect(
  state => ({
    username: state.getIn(['auth', 'currentUser', 'name']),
    // events: state.getIn(['journal', 'events']),
  })
)(StatusView);

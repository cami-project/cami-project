import {connect} from 'react-redux';

import JournalView from './JournalView';

export default connect(
  state => ({
    username: state.getIn(['auth', 'currentUser', 'name']),
    events: state.getIn(['journal', 'events']),
    refreshing: state.getIn(['journal', 'refreshing']),
  })
)(JournalView);

import {connect} from 'react-redux';

import HomepageView from './HomepageView';

export default connect(
  state => ({
    notification: state.getIn(['homepage', 'notification']),
    username: state.getIn(['auth', 'currentUser', 'name'])
  })
)(HomepageView);

import {connect} from 'react-redux';

import HomepageView from './HomepageView';

export default connect(
  state => ({
    username: state.getIn(['auth', 'currentUser', 'name']),
    status: state.getIn(['homepageCaregiver']),
  })
)(HomepageView);

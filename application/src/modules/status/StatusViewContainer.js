import {connect} from 'react-redux';

import StatusView from './StatusView';

export default connect(
  state => ({
    username: state.getIn(['auth', 'currentUser', 'name']),
    status: state.getIn(['status']),
    weight: state.getIn(['homepageCaregiver', 'weight'])
  })
)(StatusView);

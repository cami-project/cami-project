import {connect} from 'react-redux';

import ActivitiesView from './ActivitiesView';

export default connect(
  state => ({
    username: state.getIn(['auth', 'currentUser', 'name']),
    events: state.getIn(['activities', 'events'])
  })
)(ActivitiesView);

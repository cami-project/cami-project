import {connect} from 'react-redux';

import HomepageView from './HomepageView';
import {redirectToLoginPage} from '../navigation/NavigationState';
import {logout} from '../auth/AuthState';

export default connect(
  state => ({
    notification: state.getIn(['homepage', 'notification']),
    username: state.getIn(['auth', 'currentUser', 'name'])
  }),
  dispatch => ({
    logout() {
      dispatch(logout());
      dispatch(redirectToLoginPage());
    }
  })
)(HomepageView);

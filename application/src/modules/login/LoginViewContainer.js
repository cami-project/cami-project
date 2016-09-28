import {connect} from 'react-redux';

import LoginView from './LoginView';

export default connect(
  state => ({
    username: state.getIn(['auth', 'currentUser', 'name'])
  })
)(LoginView);

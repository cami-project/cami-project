import {connect} from 'react-redux';

import OnboardingView from './OnboardingView';

export default connect(
  state => ({
    username: state.getIn(['auth', 'currentUser', 'name'])
  })
)(OnboardingView);

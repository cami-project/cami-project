import {connect} from 'react-redux';

import StatusView from './StatusView';

export default connect(
  state => ({
    username: state.getIn(['auth', 'currentUser', 'name']),
    status: state.getIn(['status']),
    weight: state.getIn(['homepageCaregiver', 'weight']),
    heart_rate: state.getIn(['homepageCaregiver', 'heart_rate']),
    steps: state.getIn(['homepageCaregiver', 'steps']),
    bp_diastolic: state.getIn(['homepageCaregiver', 'bp_diastolic']),
    bp_systolic: state.getIn(['homepageCaregiver', 'bp_systolic']),
    bp_pulse: state.getIn(['homepageCaregiver', 'bp_pulse'])
  })
)(StatusView);

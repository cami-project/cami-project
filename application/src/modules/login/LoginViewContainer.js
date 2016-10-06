import {connect} from 'react-redux';
import {goToCaregiverPage, goToElderlyPage} from '../navigation/NavigationState';
import LoginView from './LoginView';

export default connect(
  state => ({
    username: state.getIn(['auth', 'currentUser', 'name']),
    navigationState: state.get('navigationState').toJS()
  })
)(LoginView);

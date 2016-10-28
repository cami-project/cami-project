import {connect} from 'react-redux';
import {popRoute, switchTab, navigationCompleted, redirectToLoginPage} from './NavigationState';
import NavigationView from './NavigationView';
import {logout} from '../auth/AuthState';

export default connect(
  state => ({
    navigationState: state.get('navigationState').toJS()
  }),
  dispatch => ({
    switchTab(index) {
      dispatch(switchTab(index));
    },
    onNavigateBack() {
      dispatch(popRoute());
    },
    onNavigateCompleted() {
      dispatch(navigationCompleted());
    },
    logout() {
      dispatch(logout());
      dispatch(redirectToLoginPage());
    }
  })
)(NavigationView);

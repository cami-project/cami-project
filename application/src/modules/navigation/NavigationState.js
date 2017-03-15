import {fromJS} from 'immutable';

// Actions
const PUSH_ROUTE = 'NavigationState/PUSH_ROUTE';
const POP_ROUTE = 'NavigationState/POP_ROUTE';
const SWITCH_TAB = 'NavigationState/SWITCH_TAB';
const NAVIGATION_COMPLETED = 'NavigationState/NAVIGATION_COMPLETED';

export function switchTab(index) {
  return {
    type: SWITCH_TAB,
    payload: index
  };
}

// Action creators
export function pushRoute(state) {
  return (dispatch, getState) => {
    // conditionally execute push to avoid double
    // navigations due to impatient users
    if (!isNavigationAnimationInProgress(getState())) {
      dispatch({type: PUSH_ROUTE, payload: state});
    }
  };
}

export function popRoute() {
  return {type: POP_ROUTE};
}

export function navigationCompleted() {
  return {type: NAVIGATION_COMPLETED};
}

// TODO: find a better way to navigate to the page than hardcoding the indexes from the routes
const LoginPageIndex = 0;
const CaregiverPageIndex = 1;
const ElderlyPageIndex = 4;
const OnboardingPageIndex = 5;
const LogoutPageIndex = 6;

export function isLogoutPageIndex(index) {
  return index == LogoutPageIndex;
}
export function redirectToLoginPage() {
  return switchTab(LoginPageIndex);
}
export function redirectToCaregiverPage() {
  return switchTab(CaregiverPageIndex);
}
export function redirectToOnboardingPage() {
  return switchTab(OnboardingPageIndex);
}
export function redirectToElderlyPage() {
  return switchTab(ElderlyPageIndex);
}

const initialState = fromJS(
  createNavigationState('MainNavigation', 'App', '', [
    createNavigationState('Login', 'Login', 'ios-person', [{key: 'Login', title: 'Login'}], false),
    createNavigationState('HomepageCaregiver', 'Home', 'ios-home', [{key: 'HomepageCaregiver', title: 'Home'}], true),
    createNavigationState('Status', 'Status', 'ios-pulse', [{key: 'Status', title: 'Status'}], true),
    createNavigationState('Journal', 'Journal', 'ios-paper', [{key: 'Journal', title: 'Journal'}], true),
    createNavigationState('Activities', 'Activities', 'ios-calendar', [{key: 'Activities', title: 'Activities'}], true),
    createNavigationState('HomepageTab', 'Settings', 'ios-cog', [{key: 'Homepage', title: 'Homepage'}], false),
    createNavigationState('Onboarding', 'Onboard', 'ios-help-buoy', [{key: 'Onboarding', title: 'Onboard'}], false),
    createNavigationState('Logout', 'Logout', 'ios-log-out', [{key: 'Logout', title: 'Logout'}], true)
  ]));

export default function NavigationReducer(state = initialState, action) {
  switch (action.type) {
    case PUSH_ROUTE:
      return state
        .set('isNavigating', true)
        .updateIn(['routes', state.get('index')], tabState =>
          tabState
            .update('routes', routes => routes.push(fromJS(action.payload)))
            .set('index', tabState.get('routes').size));

    case POP_ROUTE:
      return state
        .set('isNavigating', true)
        .updateIn(['routes', state.get('index')], tabState =>
          tabState
            .update('routes', routes => routes.pop())
            .set('index', tabState.get('routes').size - 2));

    case SWITCH_TAB:
      return state.set('index', action.payload);

    case NAVIGATION_COMPLETED:
      return state.set('isNavigating', false);

    default:
      return state;
  }
}

// Helper for creating a state object compatible with the
// RN NavigationExperimental navigator
function createNavigationState(key, title, icon, routes, showInTabBar) {
  return {
    key,
    title,
    icon,
    index: 0,
    routes,
    showInTabBar
  };
}

function isNavigationAnimationInProgress(state) {
  return state.getIn(['navigationState', 'isNavigating']);
}

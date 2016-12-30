import {fromJS} from 'immutable';

var json = require('../../../api-examples/homepage/severity.low.json');

// Initial state
const initialState = fromJS(json);

// Actions
const OPEN_CAREGIVER_PAGE = 'LoginState/OPEN_CAREGIVER_PAGE';
const OPEN_ELDERLY_PAGE = 'LoginState/OPEN_ELDERLY_PAGE';

export function logIn(username, password) {
  // TODO: check username & password
  // and redirect the user to the corresponding page
  return {
    type: OPEN_CAREGIVER_PAGE
  };
}

// Reducer
export default function LoginStateReducer(state = initialState, action = {}) {
  switch (action.type) {
    // TODO: open actual pages
    case OPEN_CAREGIVER_PAGE:
      return state;
    case OPEN_ELDERLY_PAGE:
      return state;
    default:
      return state;
  }
}

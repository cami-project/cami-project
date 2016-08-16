import {fromJS, Map} from 'immutable';

import * as env from '../../../env';

var json = require('../../../api-examples/homepage-caregiver/eldery-status.json');

// Initial state
const initialState = Map({
  status: fromJS(json),
  actionability: Map({
    visible: false,
    params: {}
  })
});

// Actions
const SHOW_ACTIONABILITY = 'HomepageState/SHOW_ACTIONABILITY';
const HIDE_ACTIONABILITY = 'HomepageState/HIDE_ACTIONABILITY';

// Action creators
export function showActionability(name, icon, timestamp, message, description) {
  return {
    type: SHOW_ACTIONABILITY,
    payload: {
      name,
      icon,
      timestamp,
      message,
      description
    }
  };
}

export function hideActionability() {
  return {
    type: HIDE_ACTIONABILITY,
  };
}

// Reducer
export default function HomepageStateReducer(state = initialState, action = {}) {
  switch (action.type) {
    case SHOW_ACTIONABILITY:
      return state.setIn(['actionability', 'visible'], true)
        .setIn(['actionability', 'params'], action.payload);
    case HIDE_ACTIONABILITY:
      return state.setIn(['actionability', 'visible'], false);
    default:
      return state;
  }
}

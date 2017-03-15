import {fromJS} from 'immutable';

var json = require('../../../api-examples/activities/google-calendar-feed.json');

// Initial State
const initialState = fromJS(json);

// Reducer
export default function ActivitiesStateReducer(state = initialState, action = {}) {
  switch (action.type) {
    default:
      return state;
  }
}

import {fromJS} from 'immutable';

var json = require('../../../api-examples/homepage/severity.low.json');

// Initial state
const initialState = fromJS(json);


// Reducer
export default function LoginStateReducer(state = initialState, action = {}) {
  switch (action.type) {
    default:
      return state;
  }
}

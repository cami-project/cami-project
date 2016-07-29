// import Promise from 'bluebird';
import {fromJS} from 'immutable';
// import {loop, Effects} from 'redux-loop';

// import * as env from '../../../env';

var json = require('../../../api-examples/status/status.json');

// Initial state
const initialState = fromJS(json);

// Actions
// const DUMMY_ACTION = 'HomepageState/DUMMY_ACTION';

// Action creators

// Reducer
export default function StatusStateReducer(state = initialState, action = {}) {
  switch (action.type) {
    default:
      return state;
  }
}

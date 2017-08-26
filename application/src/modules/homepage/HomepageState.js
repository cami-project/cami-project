import Promise from 'bluebird';
import {fromJS} from 'immutable';
import {loop, Effects} from 'redux-loop';
import store from '../../redux/store';
import * as env from '../../../env';
// import store from '../../redux/store';

var json = require('../../../api-examples/homepage/severity.medium.json');

// Initial state
const initialState = fromJS(json);

// Actions
const NOTIFICATION_RESPONSE = 'HomepageState/NOTIFICATION_RESPONSE';
const TRIGGER_REQUEST = 'HomepageState/TRIGGER_REQUEST';


// Action creators
export async function requestNotification() {
  // Do an async fetch fot the latest notification.
  return {
    type: NOTIFICATION_RESPONSE,
    payload: await fetchNotification()
  };
}

async function fetchNotification() {
  // getting the user id from state after auth0 signin
  var user_id = store.getState().get('auth').get('currentUser').get('userMetadata').get('user_id');
  var apiUrl = env.NOTIFICATIONS_REST_API + "?user=" + user_id;

  // Use random parameter to defeat cache.
  apiUrl += apiUrl.indexOf('?') > -1 ? '&' : '?';
  apiUrl += 'r=' + Math.floor(Math.random() * 10000);

  console.log('[HomepageState] - Fetching data for [elder] with user ID: ' + user_id);

  return fetch(apiUrl)
    .then((response) => {
      return response.json().then(function(json) {
        if (json.objects.length == 0) {
          return initialState.getIn(['notification']);
        }

        console.log('[HomepageState] - sucessfully fetched [elder] data');
        return json.objects[0];
      });
    })
    .catch((error) => {
      console.warning('[HomepageState] - error encountered when fetching [elder] data: ' + error);
      return initialState.getIn(['notification']);
    });
}

// Simulates a periodic timer. This is for experimental purposes only, a proper timer should be used instead in
// production.
async function triggerFetchNotification() {
  return Promise.delay(env.POLL_INTERVAL_MILLIS).then(() => ({
    type: TRIGGER_REQUEST
  }))
}

// Reducer
export default function HomepageStateReducer(state = initialState, action = {}) {
  switch (action.type) {
    case TRIGGER_REQUEST:
      // State doesn't change, we just want to trigger a notification fetch.
      return loop(
        state,
        Effects.promise(requestNotification)
      );

    case NOTIFICATION_RESPONSE:
      // We got a notification update so let's update the state and then restart the timer.
      return loop(
        // TODO(@iprunache) stop triggering a render for every poll when the payload does not change; happens with immutable too.
        state.set('notification', fromJS(action.payload)),
        Effects.promise(triggerFetchNotification)
      );

    default:
      return state;
  }
}

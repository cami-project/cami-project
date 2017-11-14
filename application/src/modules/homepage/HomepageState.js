import Promise from 'bluebird';
import {fromJS, Map} from 'immutable';
import {loop, Effects} from 'redux-loop';
import store from '../../redux/store';
import moment from 'moment';
import * as env from '../../../env';

var json = require('../../../api-examples/homepage/default-severity.medium.json');

// Initial state
const initialState = Map({
  'notification': fromJS(json).get('notification')
});


// Actions
const NOTIFICATION_RESPONSE = 'HomepageState/NOTIFICATION_RESPONSE';
const TRIGGER_REQUEST = 'HomepageState/TRIGGER_REQUEST';
const ACK_RESPONSE = 'HomepageState/ACK_RESPONSE';


// Action creators
export async function requestNotification() {
  console.log('[HomepageState] - Requesting [elder] data fetch.');
  // Do an async fetch fot the latest notification.
  return {
    type: NOTIFICATION_RESPONSE,
    payload: await fetchNotification()
  };
}

export async function ackReminder(ack, reference_id, journal_entry_id) {
  return {
    type: ACK_RESPONSE,
    payload: await postReminderAcknowledgement(ack, reference_id, journal_entry_id)
  }
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

        console.log('[HomepageState] - sucessfully fetched [elder] data: ' + JSON.stringify(json.objects[0]));
        return json.objects[0];
      });
    })
    .catch((error) => {
      console.warning('[HomepageState] - error encountered when fetching [elder] data: ' + error);
      return initialState.getIn(['notification']);
    });
}

async function postReminderAcknowledgement(ack, reference_id, journal_entry_id) {
  var user_id = store.getState().get('auth').get('currentUser').get('userMetadata').get('user_id');
  var timestamp = moment().format('X');

  // Posting an event on the Insertion queue on acknowledgement
  // - This can be picked up by any services that are listening to the it, and
  //   trigger custom actions based on the User Input
  return fetch(env.INSERTION_ENDPOINT + 'events/', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        "category": "user_notifications",
        "content": {
          "name": "reminder_acknowledged",
          "value_type": "complex",
          "value": {
            "ack": ack,
            "user": { "id": user_id },
            "journal": { "id": journal_entry_id },
            "activity": { "id": reference_id }
          },
          "annotations": {
            "timestamp": timestamp,
            "source": "ios_app"
          }
        }
      })
    }).then((response) => {
      if (response.status >= 200 && response.status < 300) {
        console.log('[HomepageState] - Successful send of reminder acknowledgement event: ' + JSON.stringify(response));

        // Changing status of acknowledged field of the Journal Entry
        // - We need to remember the acknowledged state of a reminder so that we
        // no longer allow users to re-ack it
        return fetch(env.NOTIFICATIONS_REST_API + journal_entry_id + '/', {
          method: 'PATCH',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({
            "acknowledged": ack === 'ok' ? true : false
          })
        }).then((response) => {
          if (response.status >= 200 && response.status < 300) {
            console.log('[HomepageState] - Journal entry acknowledge field patched successfully: ' + JSON.stringify(response));

            return response;

          } else {
            let error = new Error(response.statusText);
            error.response = response;
            throw error;
          }

        }).catch((error) => {
          console.log('[HomepageState] - There was a problem patching the Journal Entry acknowledged field: ' + JSON.stringify(error));
        });

      } else {
        let error = new Error(response.statusText);
        error.response = response;
        throw error;
      }
    }).catch((error) => {
      console.log('[HomepageState] - There was a problem posting a [' + ack + '] ack on the insertion queue: ' + JSON.stringify(error));
    });
}

// Simulates a periodic timer. This is for experimental purposes only, a proper
// timer should be used instead in production.
async function triggerFetchNotification() {
  console.log('[HomepageState] - Triggered data fetch for [elder] in ' + (env.POLL_INTERVAL_MILLIS / 1000) + ' seconds.');

  return Promise.delay(env.POLL_INTERVAL_MILLIS).then(() => ({
    type: TRIGGER_REQUEST
  }))
}

// Reducer
export default function HomepageStateReducer(state = initialState, action = {}) {
  switch (action.type) {
    case TRIGGER_REQUEST:
      // State doesn't change, we just want to trigger a notification fetch.
      // - checking if a user is still logged to see if a new fetch should be triggered
      if (store.getState().get('auth').get('isLoggedIn')) {
        return loop(
          state,
          Effects.promise(requestNotification)
        );
      } else {
        return state;
      }
    case NOTIFICATION_RESPONSE:
      // We got a notification update so let's update the state and then restart the timer.
      // - checking if a user is still logged to see if a new fetch should be triggered
      if (store.getState().get('auth').get('isLoggedIn')) {
        return loop(
          // TODO(@iprunache) stop triggering a render for every poll when the payload does not change; happens with immutable too.
          state.setIn(['notification'], fromJS(action.payload)),
          Effects.promise(triggerFetchNotification)
        );
      } else {
        return state;
      }
    case ACK_RESPONSE:
      return state.setIn(['notification', 'acknowledged'], true);
    default:
      return state;
  }
}

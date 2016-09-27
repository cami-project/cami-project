import Promise from 'bluebird';
import {fromJS, Map} from 'immutable';
import {loop, Effects} from 'redux-loop';
import icons from 'Cami/src/icons-fa';

import * as env from '../../../env';

var json = require('../../../api-examples/homepage-caregiver/eldery-status.json');
var previousNotificationId = "";

// Initial state
const initialState = Map({
  status: Map({
    visible: false,
    values: fromJS(json)
  }),
  actionability: Map({
    visible: false,
    params: {}
  })
});

// Actions
const HIDE_ACTIONABILITY = 'HomepageState/HIDE_ACTIONABILITY';
const CAREGIVER_DATA_RESPONSE = 'HomepageState/CAREGIVER_DATA_RESPONSE';
const CAREGIVER_DATA_REQUEST = 'HomepageState/CAREGIVER_DATA_REQUEST';

export function hideActionability() {
  return {
    type: HIDE_ACTIONABILITY,
  };
}

export async function requestCaregiverData() {
  return {
    type: CAREGIVER_DATA_RESPONSE,
    payload: await fetchPageData()
  };
}

async function fetchPageData() {
  var notificationsUrl = env.NOTIFICATIONS_REST_API + "?recipient_type=caregiver&limit=1&offset=0";
  notificationsUrl += '&r=' + Math.floor(Math.random() * 10000);
  var result = {
    hasNotification: false,
    weight: {
      "status": "ok",
      "amount": []
    }
  };
  return fetch(notificationsUrl)
    .then((response) => response.json())
    .then((notificationJson) => {
      var notificationList = notificationJson.objects;
      if (notificationList.length > 0) {
        var receivedNotification = notificationList[0];
        result.notification = {
          id: receivedNotification.id,
          name: "Jim",
          icon: icons[receivedNotification.type],
          timestamp: parseInt(receivedNotification.timestamp),
          message: receivedNotification.message,
          description: receivedNotification.description
        }
        result.hasNotification = true;
      }
      var apiUrl = env.WEIGHT_MEASUREMENTS_LAST_VALUES;
      return fetch(apiUrl).then((response) => response.json())
        .then((weightsJson) => {
          result.weight = weightsJson.weight;
          return result;
        });
    }).catch((error) => {
      return result;
    });
}
async function triggerFetchPageData() {
  return Promise.delay(env.POLL_INTERVAL_MILLIS).then(() => ({
    type: CAREGIVER_DATA_REQUEST
  }))
}

// Reducer
export default function HomepageStateReducer(state = initialState, action = {}) {
  switch (action.type) {
    case HIDE_ACTIONABILITY:
      return state.setIn(['actionability', 'visible'], false);

    case CAREGIVER_DATA_REQUEST:
      return loop(
        state,
        Effects.promise(requestCaregiverData)
      );

    case CAREGIVER_DATA_RESPONSE:
      var chartValuesJson = json;
      chartValuesJson['weight'] = action.payload.weight;

      var isVisible = state.getIn(['actionability', 'visible']);
      if (!isVisible) {
        isVisible = action.payload.hasNotification && previousNotificationId !== action.payload.notification.id;
        previousNotificationId = action.payload.hasNotification ? action.payload.notification.id : previousNotificationId;
      }

      return loop(
        state.setIn(['actionability', 'visible'], isVisible)
          .setIn(['actionability', 'params'], action.payload.notification)
          .setIn(['status', 'visible'], true)
          .setIn(['status', 'values'], fromJS(chartValuesJson)),
        Effects.promise(triggerFetchPageData)
      );

    default:
      return state;
  }
}
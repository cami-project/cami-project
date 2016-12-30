// import Promise from 'bluebird';
import {fromJS, Map} from 'immutable';
// import {loop, Effects} from 'redux-loop';

import * as env from '../../../env';

// Initial state
const initialState = Map({
  events: fromJS([]),
  refreshing: false
});

const JOURNAL_DATA_RESPONSE = 'JournalState/JOURNAL_DATA_RESPONSE';
const START_REFRESHING = 'JournalState/START_REFRESHING';

export function markStartRefreshing() {
  return {
    type: START_REFRESHING
  };
}

export async function requestJournalData() {
  return {
    type: JOURNAL_DATA_RESPONSE,
    payload: await fetchPageData()
  };
}
async function fetchPageData() {
  var notificationsUrl = env.NOTIFICATIONS_REST_API + "?recipient_type=caregiver&limit=30&offset=0";
  notificationsUrl += '&r=' + Math.floor(Math.random() * 10000);

  return fetch(notificationsUrl)
    .then((response) => response.json())
    .then((notificationJson) => {
      var notifications = [];
      var notificationJsonList = notificationJson.objects;
      notificationJsonList.forEach((notification) => {
        notifications.push({
          type: notification.type,
          status: notification.severity,
          timestamp: parseInt(notification.timestamp),
          title: notification.message,
          message: notification.description
        });
      });
      return notifications;
    }).catch((error) => {
      return [];
    });
}

// Reducer
export default function JournalStateReducer(state = initialState, action = {}) {
  switch (action.type) {
    case START_REFRESHING:
      return state.setIn(['refreshing'], true);
    case JOURNAL_DATA_RESPONSE:
      return state.setIn(['refreshing'], false)
          .setIn(['events'], fromJS(action.payload))
    default:
      return state;
  }
}

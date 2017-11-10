import Promise from 'bluebird';
import {fromJS, Map} from 'immutable';
import {loop, Effects} from 'redux-loop';
import store from '../../redux/store';
import icons from 'Cami/src/icons-fa';

import * as env from '../../../env';

var json = require('../../../api-examples/homepage-caregiver/eldery-status.json');
var activitiesInitialJson = require('../../../api-examples/activities/google-calendar-feed-from-api.json');
var previousNotificationId = "";

// Initial state
const initialState = Map({
  status: Map({
    visible: false,
    values: fromJS(json)
  }),
  actionability: Map({
    visible: false,
    params: fromJS({})
  }),
  lastEvents: fromJS([]),
  lastActivities: fromJS(activitiesInitialJson),
  weight: fromJS({
    "status": "ok",
    "amount": [],
    "data": [],
    "threshold": 72,
  }),
  heart_rate: fromJS({
    "status": "ok",
    "amount": [],
    "data": [],
    "threshold": 80
  }),
  steps: fromJS({
    "status": "ok",
    "amount": [],
    "data": [],
    "threshold": 150
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
  // getting the user id from state after auth0 signin
  var user_id = store.getState().get('auth').get('currentUser').get('userMetadata').get('user_id');
  // getting the elder's id that's associated w/ the current caregiver
  var elder_id = store.getState().get('auth').get('currentUser').get('userMetadata').get('elder_id');
  var notificationsUrl = env.NOTIFICATIONS_REST_API + "?user=" + user_id;
  // override caching issues
  notificationsUrl += '&r=' + Math.floor(Math.random() * 10000);

  var result = {
    hasNotification: false,
    weight: {
      "status": "ok",
      "amount": [],
      "data": [],
      "threshold": 72,
    },
    heart_rate: {
      "status": "ok",
      "amount": [],
      "data": [],
      "threshold": 80
    },
    steps: {
      "status": "ok",
      "amount": [],
      "data": [],
      "threshold": 150
    },
    lastEvents: [],
    lastActivities: []
  };

  console.log("[HomepageState] - fetching data for the [careviger] with id: " + user_id);

  return fetch(notificationsUrl)
    .then((response) => response.json())
    .then((notificationJson) => {

      var notificationList = notificationJson.objects;

      if (notificationList.length > 0) {

        var receivedNotification = notificationList[0];

        result.notification = {
          id: receivedNotification.id,
          name: "Loved one",
          icon: icons[receivedNotification.type],
          timestamp: parseInt(receivedNotification.timestamp),
          message: receivedNotification.message,
          description: receivedNotification.description
        }

        notificationList.forEach((notification) => {
          result.lastEvents.push({
            type: notification.type,
            status: notification.severity,
            timestamp: parseInt(notification.timestamp),
            title: notification.message,
            message: notification.description
          });
        });

        result.hasNotification = true;
      }

      var weightApiUrl = env.WEIGHT_MEASUREMENTS_LAST_VALUES + '?user=' + elder_id;
      var heartRateApiUrl = env.HEARTRATE_MEASUREMENTS_LAST_VALUES + '?user=' + elder_id;
      var stepsCountApiUrl = env.STEPS_MEASUREMENTS_LAST_VALUES + '?user=' + elder_id + '&units=7&resolution=days';
      var activitiesApiUrl = env.ACTIVITIES_LAST_EVENTS + '?user=' + elder_id;

      return fetch(activitiesApiUrl).then((response) => response.json())
        .then((activitiesJson) => {

          console.log('[HomepageState] - Activities Payload: ' + JSON.stringify(activitiesJson));
          result.lastActivities = activitiesJson;

          return fetch(weightApiUrl).then((response) => response.json())
            .then((weightsJson) => {

              console.log('[HomepageState] - Weight Payload: ' + JSON.stringify(weightsJson));
              result.weight = weightsJson.weight;

              return fetch(heartRateApiUrl).then((response) => response.json())
                .then((heartRateJson) => {

                  console.log('[HomepageState] - Heart Rate Payload: ' + JSON.stringify(heartRateJson));
                  result.heart_rate = heartRateJson.heart_rate;

                  return fetch(stepsCountApiUrl).then((response) => response.json())
                    .then((stepsCountJson) => {
                      result.steps = stepsCountJson.steps;

                      console.log('[HomepageState] - Steps Count Payload: ' + JSON.stringify(stepsCountJson));
                      console.log('[HomepageState] - Successfully fetched the [caregiver] data');

                      return result;

                  }).catch((error) => {
                    console.warning('[HomepageState] - encountered error while fetching [caregiver] step count: ' + error);

                    return result;
                  });
              }).catch((error) => {
                console.warning('[HomepageState] - encountered error while fetching [caregiver] heart rate: ' + error);

                return result;
              });
          }).catch((error) => {
            console.warning('[HomepageState] - encountered error while fetching [caregiver] weight: ' + error);

            return result;
          });
      }).catch((error) => {
        console.warning('[HomepageState] - encountered error while fetching [caregiver] activities: ' + error);

        return result;
      });
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
      chartValuesJson['heart_rate'] = action.payload.heart_rate;
      chartValuesJson['steps'] = action.payload.steps;

      var isVisible = state.getIn(['actionability', 'visible']);
      if (!isVisible) {
        isVisible = action.payload.hasNotification && previousNotificationId !== action.payload.notification.id;
        previousNotificationId = action.payload.hasNotification ? action.payload.notification.id : previousNotificationId;
      }

      return loop(
        state.setIn(['actionability', 'visible'], isVisible)
          .setIn(['actionability', 'params'], Map(fromJS(action.payload.notification)))
          .setIn(['status', 'visible'], true)
          .setIn(['status', 'values'], fromJS(chartValuesJson))
          .setIn(['weight'], fromJS(action.payload.weight))
          .setIn(['heart_rate'], fromJS(action.payload.heart_rate))
          .setIn(['steps'], fromJS(action.payload.steps))
          .setIn(['lastEvents'], fromJS(action.payload.lastEvents))
          .setIn(['lastActivities'], fromJS(action.payload.lastActivities)),
        Effects.promise(triggerFetchPageData)
      );

    default:
      return state;
  }
}

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
  }),
  bp_systolic: fromJS({
    "status": "ok",
    "amount": [],
    "data": [],
    "threshold": 118.2
  }),
  bp_diastolic: fromJS({
    "status": "ok",
    "amount": [],
    "data": [],
    "threshold": 77.25
  }),
  bp_pulse: fromJS({
    "status": "ok",
    "amount": [],
    "data": [],
    "threshold": 69.45
  }),
  bp_aggregated: fromJS({
    "status": "ok",
    "amount": [],
    "data": [],
    "threshold": "117.8/77.1"
  }),
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
    bp_systolic: {
      "status": "ok",
      "amount": [],
      "data": [],
      "threshold": 118.2
    },
    bp_diastolic: {
      "status": "ok",
      "amount": [],
      "data": [],
      "threshold": 77.25
    },
    bp_pulse: {
      "status": "ok",
      "amount": [],
      "data": [],
      "threshold": 69.45
    },
    bp_aggregated: {
      "status": "ok",
      "amount": [],
      "data": [],
      "threshold": "117.8/77.1"
    },
    lastEvents: [],
    lastActivities: []
  };

  console.log("[HomepageState] - fetching data for the [careviger] with id: " + user_id);

  return fetch(notificationsUrl).then((response) => response.json()).then((notificationJson) => {

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

    var weightApiUrl = env.WEIGHT_MEASUREMENTS_LAST_VALUES + '?user=' + elder_id,
        heartRateApiUrl = env.HEARTRATE_MEASUREMENTS_LAST_VALUES + '?user=' + elder_id,
        bpDiastolicApiUrl = env.BP_DIASTOLIC_MEASUREMENTS_LAST_VALUES + '?user=' + elder_id,
        bpSystolicApiUrl = env.BP_SYSTOLIC_MEASUREMENTS_LAST_VALUES + '?user=' + elder_id,
        bpPulseApiUrl = env.BP_PULSE_MEASUREMENTS_LAST_VALUES + '?user=' + elder_id,
        bpAggregatedApiUrl = env.BP_AGGREGATED_MEASUREMENTS_LAST_VALUES + '?user=' + elder_id,
        stepsCountApiUrl = env.STEPS_MEASUREMENTS_LAST_VALUES + '?user=' + elder_id + '&units=7&resolution=days',
        activitiesApiUrl = env.ACTIVITIES_LAST_EVENTS + '?user=' + elder_id;

    return fetch(activitiesApiUrl)
      .then((response) => response.json())
      .then((activitiesJson) => {

      console.log('[HomepageState] - Activities Payload: ' + JSON.stringify(activitiesJson));
      result.lastActivities = activitiesJson;

      return fetch(weightApiUrl)
        .then((response) => response.json())
        .then((weightsJson) => {

        console.log('[HomepageState] - Weight Payload: ' + JSON.stringify(weightsJson));
        result.weight = weightsJson.weight;

        return fetch(heartRateApiUrl)
          .then((response) => response.json())
          .then((heartRateJson) => {

          console.log('[HomepageState] - Heart Rate Payload: ' + JSON.stringify(heartRateJson));
          result.heart_rate = heartRateJson.heart_rate;

          return fetch(stepsCountApiUrl)
            .then((response) => response.json())
            .then((stepsCountJson) => {

            console.log('[HomepageState] - Steps Count Payload: ' + JSON.stringify(stepsCountJson));
            result.steps = stepsCountJson.steps;

            return fetch(bpDiastolicApiUrl)
              .then((response) => response.json())
              .then((diastolicJson) => {

              console.log('[HomepageState] - BP Diastolic Payload: ' + JSON.stringify(diastolicJson));
              result.bp_diastolic = diastolicJson.heart_rate;

              return fetch(bpSystolicApiUrl)
                .then((response) => response.json())
                .then((systolicJson) => {

                console.log('[HomepageState] - BP Systolic Payload: ' + JSON.stringify(systolicJson));
                result.bp_systolic = systolicJson.heart_rate;

                return fetch(bpPulseApiUrl)
                  .then((response) => response.json())
                  .then((pulseJson) => {

                  console.log('[HomepageState] - BP Pulse Payload: ' + JSON.stringify(pulseJson));
                  result.bp_pulse = pulseJson.heart_rate;

                  return fetch(bpAggregatedApiUrl)
                    .then((response) => response.json())
                    .then((bpJson) => {

                    console.log('[HomepageState] - BP Aggregated Payload: ' + JSON.stringify(bpJson));
                    result.bp_aggregated = bpJson.heart_rate;

                    console.log('[HomepageState] - Successfully fetched the [caregiver] data');
                    return result;

                  }).catch((error) => {
                    console.log('[HomepageState] - encountered error while fetching [caregiver] bp aggregated data: ' + error);

                    return result;
                  });

                }).catch((error) => {
                  console.log('[HomepageState] - encountered error while fetching [caregiver] bp pulse: ' + error);

                  return result;
                });

              }).catch((error) => {
                console.log('[HomepageState] - encountered error while fetching [caregiver] bp systolic: ' + error);

                return result;
              });
            }).catch((error) => {
              console.log('[HomepageState] - encountered error while fetching [caregiver] bp diastolic: ' + error);

              return result;
            });
          }).catch((error) => {
            console.log('[HomepageState] - encountered error while fetching [caregiver] step count: ' + error);

            return result;
          });
        }).catch((error) => {
          console.log('[HomepageState] - encountered error while fetching [caregiver] heart rate: ' + error);

          return result;
        });
      }).catch((error) => {
        console.log('[HomepageState] - encountered error while fetching [caregiver] weight: ' + error);

        return result;
      });
    }).catch((error) => {
      console.log('[HomepageState] - encountered error while fetching [caregiver] activities: ' + error);

      return result;
    });
  }).catch((error) => {
    console.log('[HomepageState] - encountered error while fetching [caregiver] journal entries: ' + error);

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
      // - checking if a user is still logged to see if a new fetch should be triggered
      if (store.getState().get('auth').get('isLoggedIn')) {
        return loop(
          state,
          Effects.promise(requestCaregiverData)
        );
      } else {
        return state;
      }

    case CAREGIVER_DATA_RESPONSE:
      // - checking if a user is still logged to see if a new fetch should be triggered
      if (store.getState().get('auth').get('isLoggedIn')) {
        var chartValuesJson = json;
        chartValuesJson['weight'] = action.payload.weight;
        chartValuesJson['heart_rate'] = action.payload.heart_rate;
        chartValuesJson['bp_diastolic'] = action.payload.bp_diastolic;
        chartValuesJson['bp_systolic'] = action.payload.bp_systolic;
        chartValuesJson['bp_pulse'] = action.payload.bp_pulse;
        chartValuesJson['bp_aggregated'] = action.payload.bp_aggregated;
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
            .setIn(['bp_diastolic'], fromJS(action.payload.bp_diastolic))
            .setIn(['bp_systolic'], fromJS(action.payload.bp_systolic))
            .setIn(['bp_pulse'], fromJS(action.payload.bp_pulse))
            .setIn(['bp_aggregated'], fromJS(action.payload.bp_aggregated))
            .setIn(['steps'], fromJS(action.payload.steps))
            .setIn(['lastEvents'], fromJS(action.payload.lastEvents))
            .setIn(['lastActivities'], fromJS(action.payload.lastActivities)),
          Effects.promise(triggerFetchPageData)
        );
      } else {
        return state;
      }

    default:
      return state;
  }
}

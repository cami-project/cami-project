import {fromJS, Map} from 'immutable';

import * as env from '../../../env';

var json = require('../../../api-examples/homepage-caregiver/eldery-status.json');

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
const SHOW_ACTIONABILITY = 'HomepageState/SHOW_ACTIONABILITY';
const HIDE_ACTIONABILITY = 'HomepageState/HIDE_ACTIONABILITY';
const SHOW_CHART_DATA = 'HomepageState/SHOW_CHART_DATA';

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

export async function initChartData() {
  // Do an async fetch fot the latest notification.
  return {
    type: SHOW_CHART_DATA,
    payload: await fetchChartData()
  };
}
async function fetchChartData() {
  var apiUrl = env.WEIGHT_MEASUREMENTS_LAST_VALUES;
  return fetch(apiUrl)
    .then((response) => {
      return response.json().then(function (weightsJson) {
        return weightsJson['weight'];
      });
    })
    .catch((error) => {
      // fallback to mockup JSON to not tear down the demo
      return json["weight"];
    });
}

// Reducer
export default function HomepageStateReducer(state = initialState, action = {}) {
  switch (action.type) {
    case SHOW_ACTIONABILITY:
      return state.setIn(['actionability', 'visible'], true)
        .setIn(['actionability', 'params'], action.payload);
    case HIDE_ACTIONABILITY:
      return state.setIn(['actionability', 'visible'], false);
    case SHOW_CHART_DATA:
      var valuesJson = json;
      valuesJson['weight'] = action.payload;
      return state.set('status', fromJS({
        visible: true,
        values: valuesJson
      }))
      
    default:
      return state;
  }
}

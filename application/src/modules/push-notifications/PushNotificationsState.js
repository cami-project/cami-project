import { Map } from 'immutable';

// Initial state
const initialState = Map({
  didReceiveKey: false
});

const RECEIVED_MOBILE_NOTIFICATION_KEY = 'PushNotificationsState/RECEIVED_MOBILE_NOTIFICATION_KEY';

export function didReceiveMobileNotificationKey(mobileNotificationKey, mobileOS) {
  return {
    type: RECEIVED_MOBILE_NOTIFICATION_KEY,
    payload: {
      mobileNotificationKey: mobileNotificationKey,
      mobileOS: mobileOS
    }
  };
}
// Reducer
export default function PushNotificationsStateReducer(state = initialState, action = {}) {
  switch (action.type) {
    case RECEIVED_MOBILE_NOTIFICATION_KEY:
      return state.setIn(['didReceiveKey'], true)
        .setIn(['mobileNotificationKey'], action.payload.mobileNotificationKey)
        .setIn(['mobileOS'], action.payload.mobileOS);
    default:
      return state;
  }
}
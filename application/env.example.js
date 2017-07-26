// Secrets for the applications. Do NOT commit any secrets to version control.
// 1. cp env.example.js env.js
// 2. Fill in the blanks

module.exports = {
  AUTH0_CLIENT_ID: 'xqN8fSjHL6850gI2m8AUkk5GICgvTCUs',
  AUTH0_DOMAIN: 'vitamin.eu.auth0.com',
  POLL_INTERVAL_MILLIS: 5000,
  NOTIFICATIONS_REST_API: 'http://cami.vitaminsoftware.com:8008/api/v1/journal_entries/',
  ACTIVITIES_LAST_EVENTS: 'http://cami.vitaminsoftware.com:8008/api/v1/activity/last_activities/',
  WEIGHT_MEASUREMENTS_LAST_VALUES: 'http://cami.vitaminsoftware.com:8000/api/v1/weight-measurements/last_values/',
  HEARTRATE_MEASUREMENTS_LAST_VALUES: 'http://cami.vitaminsoftware.com:8000/api/v1/heartrate-measurements/last_values/',
  STEPS_MEASUREMENTS_LAST_VALUES: 'http://cami.vitaminsoftware.com:8000/api/v1/steps-measurements/last_values/',
  NOTIFICATIONS_SUBSCRIPTION_API: 'http://cami.vitaminsoftware.com:8000/subscribe_notifications/',
  MOBILE_NOTIFICATION_KEY_API: 'http://cami.vitaminsoftware.com:8001/api/v1/push-notification-subscribe/'
};

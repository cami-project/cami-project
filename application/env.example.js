// Secrets for the applications. Do NOT commit any secrets to version control.
// 1. cp env.example.js env.js
// 2. Fill in the blanks

module.exports = {
  AUTH0_CLIENT_ID: '',
  AUTH0_DOMAIN: '',
  POLL_INTERVAL_MILLIS: 5000,
  NOTIFICATIONS_REST_API: 'http://139.59.181.210:8001/api/v1/notifications/',
  WEIGHT_MEASUREMENTS_LAST_VALUES: 'http://139.59.181.210:8000/api/v1/weight-measurements/last_values/'
};

from django.core.exceptions import ImproperlyConfigured
from django.utils.six import string_types
from .base import BaseConfig, check_apns_certificate
from .defaults import PUSH_NOTIFICATIONS_SETTINGS as SETTINGS

SETTING_MISMATCH = (
	"Application '{application_id}' ({platform}) does not support the setting '{setting}'."
)


# code can be "missing" or "invalid"
BAD_PLATFORM = (
	"PUSH_NOTIFICATIONS_SETTINGS.APPLICATIONS['{application_id}']['PLATFORM']"
	" is {code}. Must be one of: {platforms}."
)

UNKNOWN_PLATFORM = (
	"Unknown Platform: {platform}. Must be one of: {platforms}."
)

MISSING_SETTING = (
	"PUSH_NOTIFICATIONS_SETTINGS.APPLICATIONS['{application_id}']['{setting}'] is missing."
)

PLATFORMS = [
	"APNS",
	"FCM",
	"GCM",
	"WNS"
]

# Settings that all applications must have
REQUIRED_SETTINGS = [
	"PLATFORM",
]

# Settings that an application may have to enable optional features
# these settings are stubs for registry support and have no effect on the operation
# of the application at this time.
OPTIONAL_SETTINGS = [
	"APPLICATION_GROUP",
	"APPLICATION_SECRET",
]

APNS_REQUIRED_SETTINGS = ["CERTIFICATE"]

APNS_OPTIONAL_SETTINGS = [
	"USE_SANDBOX", "USE_ALTERNATIVE_PORT", "TOPIC"
]

FCM_REQUIRED_SETTINGS = GCM_REQUIRED_SETTINGS = ["API_KEY"]
FCM_OPTIONAL_SETTINGS = GCM_OPTIONAL_SETTINGS = [
	"POST_URL", "MAX_RECIPIENTS", "ERROR_TIMEOUT"
]

WNS_REQUIRED_SETTINGS = ["PACKAGE_SECURITY_ID", "SECRET_KEY"]

WNS_OPTIONAL_SETTINGS = ["WNS_ACCESS_URL"]


class AppConfig(BaseConfig):
	"""Supports any number of push notification enabled applications."""

	def __init__(self, settings=None):
		# supports overriding the settings to be loaded. Will load from ..settings by default.
		self._settings = settings or SETTINGS

		# initialize APPLICATIONS to an empty collection
		self._settings.setdefault("APPLICATIONS", {})

		# validate application configurations
		self._validate_applications(self._settings["APPLICATIONS"])

	def _validate_applications(self, apps):
		"""Validate the application collection"""
		for application_id, application_config in apps.items():
			self._validate_config(application_id, application_config)

			application_config["APPLICATION_ID"] = application_id

	def _validate_config(self, application_id, application_config):
		platform = application_config.get("PLATFORM", None)

		# platform is not present
		if platform is None:
			raise ImproperlyConfigured(
				BAD_PLATFORM.format(
					application_id=application_id,
					code="required",
					platforms=", ".join(PLATFORMS)
				)
			)

		# platform is not a valid choice from PLATFORMS
		if platform not in PLATFORMS:
			raise ImproperlyConfigured(
				BAD_PLATFORM.format(
					application_id=application_id,
					code="invalid",
					platforms=", ".join(PLATFORMS)
				)
			)

		validate_fn = "_validate_{platform}_config".format(platform=platform).lower()

		if hasattr(self, validate_fn):
			getattr(self, validate_fn)(application_id, application_config)
		else:
			raise ImproperlyConfigured(
				UNKNOWN_PLATFORM.format(
					platform=platform,
					platforms=", ".join(PLATFORMS)
				)
			)

	def _validate_apns_config(self, application_id, application_config):
		allowed = REQUIRED_SETTINGS + OPTIONAL_SETTINGS + APNS_REQUIRED_SETTINGS + \
			APNS_OPTIONAL_SETTINGS

		self._validate_allowed_settings(application_id, application_config, allowed)
		self._validate_required_settings(
			application_id, application_config, APNS_REQUIRED_SETTINGS
		)

		# determine/set optional values
		application_config.setdefault("USE_SANDBOX", False)
		application_config.setdefault("USE_ALTERNATIVE_PORT", False)
		application_config.setdefault("TOPIC", "")

		self._validate_apns_certificate(application_config["CERTIFICATE"])

	def _validate_apns_certificate(self, certfile):
		"""Validate the APNS certificate at startup."""

		try:
			with open(certfile, "r") as f:
				content = f.read()
				check_apns_certificate(content)
		except Exception as e:
			msg = "The APNS certificate file at %r is not readable: %s" % (certfile, e)
			raise ImproperlyConfigured(msg)

	def _validate_fcm_config(self, application_id, application_config):
		allowed = REQUIRED_SETTINGS + OPTIONAL_SETTINGS + FCM_REQUIRED_SETTINGS + \
			FCM_OPTIONAL_SETTINGS

		self._validate_allowed_settings(application_id, application_config, allowed)
		self._validate_required_settings(
			application_id, application_config, FCM_REQUIRED_SETTINGS
		)

		application_config.setdefault("POST_URL", "https://fcm.googleapis.com/fcm/send")
		application_config.setdefault("MAX_RECIPIENTS", 1000)
		application_config.setdefault("ERROR_TIMEOUT", None)

	def _validate_gcm_config(self, application_id, application_config):
		allowed = REQUIRED_SETTINGS + OPTIONAL_SETTINGS + GCM_REQUIRED_SETTINGS + \
			GCM_OPTIONAL_SETTINGS

		self._validate_allowed_settings(application_id, application_config, allowed)
		self._validate_required_settings(
			application_id, application_config, GCM_REQUIRED_SETTINGS
		)

		application_config.setdefault("POST_URL", "https://android.googleapis.com/gcm/send")
		application_config.setdefault("MAX_RECIPIENTS", 1000)
		application_config.setdefault("ERROR_TIMEOUT", None)

	def _validate_wns_config(self, application_id, application_config):
		allowed = REQUIRED_SETTINGS + OPTIONAL_SETTINGS + WNS_REQUIRED_SETTINGS + \
			WNS_OPTIONAL_SETTINGS

		self._validate_allowed_settings(application_id, application_config, allowed)
		self._validate_required_settings(
			application_id, application_config, WNS_REQUIRED_SETTINGS
		)

		application_config.setdefault("WNS_ACCESS_URL", "https://login.live.com/accesstoken.srf")

	def _validate_allowed_settings(self, application_id, application_config, allowed_settings):
		"""Confirm only allowed settings are present."""

		for setting_key in application_config.keys():
			if setting_key not in allowed_settings:
				raise ImproperlyConfigured(
					"Platform {}, app {} does not support the setting: {}.".format(
						application_config["PLATFORM"], application_id, setting_key
					)
				)

	def _validate_required_settings(
		self, application_id, application_config, required_settings
	):
		"""All required keys must be present"""

		for setting_key in required_settings:
			if setting_key not in application_config.keys():
				raise ImproperlyConfigured(
					MISSING_SETTING.format(
						application_id=application_id, setting=setting_key
					)
				)

	def _get_application_settings(self, application_id, platform, settings_key):
		"""Walks through PUSH_NOTIFICATIONS_SETTINGS to find the correct setting
		value or dies trying"""

		if not application_id:
			conf_cls = "push_notifications.conf.AppConfig"
			raise ImproperlyConfigured(
				"{} requires the application_id be specified at all times.".format(conf_cls)
			)

		# verify that the application config exists
		app_config = self._settings.get("APPLICATIONS").get(application_id, None)
		if app_config is None:
			raise ImproperlyConfigured(
				"No application configured with application_id: {}.".format(application_id)
			)

		# fetch a setting for the incorrect type of platform
		if app_config.get("PLATFORM") != platform:
			raise ImproperlyConfigured(
				SETTING_MISMATCH.format(
					application_id=application_id,
					platform=app_config.get("PLATFORM"),
					setting=settings_key
				)
			)

		# finally, try to fetch the setting
		if settings_key not in app_config:
			raise ImproperlyConfigured(
				MISSING_SETTING.format(
					application_id=application_id, setting=settings_key
				)
			)

		return app_config.get(settings_key)

	def get_gcm_api_key(self, application_id=None):
		return self._get_application_settings(application_id, "GCM", "API_KEY")

	def get_fcm_api_key(self, application_id=None):
		return self._get_application_settings(application_id, "FCM", "API_KEY")

	def get_post_url(self, cloud_type, application_id=None):
		return self._get_application_settings(application_id, cloud_type, "POST_URL")

	def get_error_timeout(self, cloud_type, application_id=None):
		return self._get_application_settings(application_id, cloud_type, "ERROR_TIMEOUT")

	def get_max_recipients(self, cloud_type, application_id=None):
		return self._get_application_settings(application_id, cloud_type, "MAX_RECIPIENTS")

	def get_apns_certificate(self, application_id=None):
		r = self._get_application_settings(application_id, "APNS", "CERTIFICATE")
		if not isinstance(r, string_types):
			# probably the (Django) file, and file path should be got
			if hasattr(r, "path"):
				return r.path
			elif (hasattr(r, "has_key") or hasattr(r, "__contains__")) and "path" in r:
				return r["path"]
			else:
				raise ImproperlyConfigured(
					"The APNS certificate settings value should be a string, or "
					"should have a 'path' attribute or key"
				)
		return r

	def get_apns_use_sandbox(self, application_id=None):
		return self._get_application_settings(application_id, "APNS", "USE_SANDBOX")

	def get_apns_use_alternative_port(self, application_id=None):
		return self._get_application_settings(application_id, "APNS", "USE_ALTERNATIVE_PORT")

	def get_apns_topic(self, application_id=None):
		return self._get_application_settings(application_id, "APNS", "TOPIC")

	def get_wns_package_security_id(self, application_id=None):
		return self._get_application_settings(application_id, "WNS", "PACKAGE_SECURITY_ID")

	def get_wns_secret_key(self, application_id=None):
		return self._get_application_settings(application_id, "WNS", "SECRET_KEY")

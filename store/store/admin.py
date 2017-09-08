from django.contrib import admin

from models import *

admin.site.register(
	[
		EndUserProfile,
		CaregiverProfile,
		HealthProfessionalProfile,
		ExternalMonitoringService,
		Device,
		DeviceUsage,
		Measurement,
		Activity,
		JournalEntry,
		PushNotificationDevice,
		Gateway
	]
)


from django.contrib import admin

from models import EndUserProfile, CaregiverProfile, HealthProfessionalProfile, \
    Activity, ExternalMonitoringService, Measurement, Device, DeviceUsage, \
    JournalEntry

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
	]
)


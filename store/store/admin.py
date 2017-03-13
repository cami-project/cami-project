from django.contrib import admin

from models import EndUserProfile, CaregiverProfile, HealthProfessionalProfile, \
    Activity, ExternalMonitoringService, Measurement, \
    Device, DeviceUsage

# admin.site.register(CaregiverProfile, CaregiverAdmin)
# admin.site.register(HealthProfessionalProfile, HealthProfessionalAdmin)

admin.site.register([EndUserProfile, CaregiverProfile, HealthProfessionalProfile])
admin.site.register([ExternalMonitoringService, Device, DeviceUsage,
                     Measurement, Activity])


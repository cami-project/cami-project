from django.contrib import admin
from models import UserAccount, Activity, ExternalMonitoringService, Measurement, Caregiver, Device, DeviceUsage

admin.site.register([UserAccount, Caregiver, ExternalMonitoringService, Device, DeviceUsage,
                     Measurement, Activity])
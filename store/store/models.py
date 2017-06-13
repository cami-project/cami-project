import uuid
import datetime

from django.db import models
from django.utils import timezone
from django_mysql.models import JSONField
from django.core.exceptions import ValidationError
from django.utils.translation import ugettext_lazy as _
from django.contrib.auth.models import User


# ============ User Information ============
class UserProfileBase(models.Model):
    class Meta:
        abstract = True

    GENDER = (
        ('M', 'Male'),
        ('F', 'Female')
    )

    LANG = (
        ('en', "English"),
        ('ro', "Romanian"),
        ('dk', "Danish"),
        ('pl', "Polish")
    )

    ACCOUNT_ROLES = (
        ('end_user', 'End User'),
        ('caregiver', 'Caregiver'),
        ('doctor', 'Doctor')
    )

    uuid = models.UUIDField(unique=True, default=uuid.uuid4)

    valid_from = models.DateField(default=timezone.now)
    valid_to = models.DateField(default=timezone.now)
    updated_at = models.DateTimeField(auto_now=True)

    account_role = models.CharField(max_length=16, choices=ACCOUNT_ROLES, default='end_user')

    gender = models.CharField(max_length=1, choices=GENDER, default='M', null=True, blank=True)
    phone = models.CharField(max_length=16, null=True, blank=True)
    address = models.CharField(max_length=256, null=True, blank=True)
    language = models.CharField(max_length=2, choices=LANG, default="en", null=True, blank=True)


class EndUserProfile(UserProfileBase):
    MARITAL_STATUS = (
        ("single", "single"),
        ("married", "married"),
        ("divorced", "divorced"),
        ("widowed", "widowed")
    )

    user = models.OneToOneField(User, on_delete=models.CASCADE, related_name='enduser_profile')

    marital_status = models.CharField(max_length=16, choices=MARITAL_STATUS, default='married', null=True, blank=True)
    age = models.PositiveIntegerField(null=True, blank=True)
    height = models.PositiveIntegerField(null=True, blank=True)

    def __str__(self):
        return "[" + self.account_role + "]" + self.user.first_name + " " + self.user.last_name + "; " + "email: " + self.user.email

    __unicode__ = __str__


class CaregiverProfile(UserProfileBase):
    user = models.OneToOneField(User, on_delete=models.CASCADE, related_name='caregiver_profile')
    caretaker = models.ForeignKey(User, related_name="caregivers")


class HealthProfessionalProfile(UserProfileBase):
    user = models.OneToOneField(User, on_delete=models.CASCADE, related_name='doctor_profile')

    title = models.CharField(max_length=32)
    affiliation = models.CharField(max_length=128)
    specialty = models.CharField(max_length=64)

    patients = models.ManyToManyField(User, related_name="doctors")


# ================ Devices ================
class Device(models.Model):

    DEVICE_TYPES = (
        ("weight", "Weight Measurement"),
        ("blood_pressure", "Blood Pressure Monitor"),
        ("pulse", "Heart Rate Monitor"),
        ("oxymeter", "Oxymeter"),
        ("pedometer", "Step Counter")
    )

    device_type = models.CharField(max_length=32, choices=DEVICE_TYPES, default="weight")
    manufacturer = models.CharField(max_length=128, null=True, blank=True)
    model = models.CharField(max_length=64, null=True, blank=True)
    serial_number = models.CharField(max_length=64)

    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)

    activation_date = models.DateTimeField(default=timezone.now)

    used_by = models.ManyToManyField(User, related_name="used_devices", through="DeviceUsage")

    def __str__(self):
        return "Device of type " + self.device_type + ": " + self.manufacturer + " " + self.model

    __unicode__ = __str__


class DeviceUsage(models.Model):
    user = models.ForeignKey(User)
    device = models.ForeignKey(Device)
    uses_since = models.DateField(auto_now=True)
    access_info = JSONField()

    def __str__(self):
        return str(self.device) + " USED BY " + str(self.user)

    __unicode__ = __str__


class ExternalMonitoringService(models.Model):
    user = models.ForeignKey(User, related_name="used_monitoring_services")

    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)

    name = models.CharField(max_length=32)
    service_url = models.URLField()

    access_info = JSONField()

    def __str__(self):
        return "ExternalMonitoringService " + self.name + " for user: " + self.user.email


# ================ Measurement Information ================
def validate_precision_range(value):
    if value < 0 or value > 100:
        raise ValidationError(_('%(value) is not a precision within allowed percentage levels: [0, 100]'),
                                  params={'value': value}, )


class Measurement(models.Model):
    MEASUREMENTS = (
        ("weight", "Weight Measurement"),
        ("blood_pressure", "Blood Pressure Measurement"),
        ("pulse", "Heart Rate Measurement"),
        ("saturation", "Blood Oxygen Saturation Measurement"),
        ("steps", "Pedometry")
    )

    MEASUREMENT_UNITS = (
        ("no_dim", "No dimension"),
        ("bpm", "Beats Per Minute"),
        ("kg", "Kilogram"),
        ("celsius", "Degrees Celsius"),
        ("mmhg", "Pressure in mm Hg")
    )

    user = models.ForeignKey(User, related_name="health_measurements")
    device = models.ForeignKey(Device, related_name="performed_measurements")
    timestamp = models.BigIntegerField()
    measurement_type = models.CharField(max_length=32, choices=MEASUREMENTS, default="weight")
    unit_type = models.CharField(max_length=8, choices=MEASUREMENT_UNITS, default="kg")
    precision = models.PositiveIntegerField(
        default=100, null=True, blank=True, validators=[validate_precision_range]
    )
    value_info = JSONField()
    ok = models.BooleanField()
    
    def __str__(self):
        return (
            "[" + self.measurement_type + "]" +
            " for user: " + self.user.first_name + " " + self.user.last_name +
            ", taken at: " + datetime.datetime.utcfromtimestamp(self.timestamp).isoformat() +
            ", value: " + str(self.value_info)
        )

    __unicode__ = __str__


# ================ User Activity Information ================
class Activity(models.Model):
    class Meta:
        verbose_name_plural = "activities"
        unique_together = ('user', 'calendar_id', 'event_id')

    ACTIVITY_TYPE = (
        ("personal", "personal"),
        ("exercise", "exercise"),
        ("medication", "medication"),
        ("measurement", "measurement")
    )

    event_id = models.CharField(max_length=64)
    user = models.ForeignKey(User)
    status = models.CharField(max_length=16, null=True, blank=True)
    html_link = models.CharField(max_length=256, null=True, blank=True)
    created = models.BigIntegerField()
    updated = models.BigIntegerField()
    title = models.CharField(max_length=64)
    description = models.CharField(max_length=256, null=True, blank=True)
    location = models.CharField(max_length=256, null=True, blank=True)
    creator = JSONField(null=True, blank=True)
    calendar_id = models.CharField(max_length=64)
    calendar_name = models.CharField(max_length=64)
    color = JSONField()
    start = models.BigIntegerField()
    end = models.BigIntegerField()
    recurring_event_id = models.CharField(max_length=64, null=True, blank=True)
    iCalUID = models.CharField(max_length=64, null=True, blank=True)
    reminders = JSONField()
    activity_type = models.CharField(max_length=16, choices=ACTIVITY_TYPE, default="personal")

    def __str__(self):
        return "[Activity] User: %s, Created: %s, Start: %s, Type: %s, Calendar Name: %s, Title: %s" % (
            self.user.first_name + " " + self.user.last_name,
            datetime.datetime.fromtimestamp(self.created).strftime('%Y-%m-%d %H:%M:%S'),
            datetime.datetime.fromtimestamp(self.start).strftime('%Y-%m-%d %H:%M:%S'),
            self.activity_type,
            self.calendar_name,
            self.title
        )

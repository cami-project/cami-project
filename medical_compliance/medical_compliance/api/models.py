from __future__ import unicode_literals

from django.db import models


# Create your models here.
class MedicationPlan(models.Model):
    class Meta:
        db_table = 'medication_plan'

    plan_id = models.AutoField(primary_key=True)
    name = models.CharField(max_length=1000)
    daily_dose = models.IntegerField()


class WithingsMeasurement(models.Model):
    WEIGHT = 'weight'
    HEIGHT = 'height'
    FAT_FREE_MASS = 'fat_free_mass'
    FAT_RATIO = 'fat_ratio'
    FAT_MASS_WEIGHT = 'fat_mass_weight'
    DIASTOLIC_BLOOD_PRESSURE = 'diastolic_blood_pressure'
    SYSTOLIC_BLOOD_PRESSURE = 'systolic_blood_pressure'
    HEART_PULSE = 'heart_pulse'

    MEASURE_CHOICES = (
        (1, WEIGHT), (4, HEIGHT), (5, FAT_FREE_MASS),
        (6, FAT_RATIO), (8, FAT_MASS_WEIGHT),
        (9, DIASTOLIC_BLOOD_PRESSURE), (10, SYSTOLIC_BLOOD_PRESSURE),
        (11, HEART_PULSE)
    )

    RETRIEVAL_CHOICES = (
        (0, 'sensed_sure'), (1, 'sensed_unsure'), (2, 'manual'),
        (4, 'manual_creation'), (5, 'auto_blood_pressure')
    )

    MEASUREMENT_SI_UNIT = {
        WEIGHT:           'kg',
        HEIGHT:           'm',
        FAT_FREE_MASS:    'kg',
        FAT_RATIO:        '%',
        FAT_MASS_WEIGHT:  'kg',
        DIASTOLIC_BLOOD_PRESSURE: 'mmHg',
        SYSTOLIC_BLOOD_PRESSURE:  'mmHg',
        HEART_PULSE:              'bpm'
    }

    withings_user_id = models.BigIntegerField()
    type = models.IntegerField(choices=MEASURE_CHOICES, default=1)
    retrieval_type = models.IntegerField(choices=RETRIEVAL_CHOICES, default=0)
    measurement_unit = models.CharField(max_length=6, default="kg")
    timestamp = models.BigIntegerField()
    timezone = models.CharField(max_length=64)
    value = models.FloatField()

    def __unicode__(self):
        import datetime, pytz

        pretty_date = datetime.datetime.fromtimestamp(self.timestamp, tz = pytz.timezone(self.timezone)).strftime("%Y-%m-%d %H:%M:%S %z")
        return u'Measurement of type: %s, value: %s %s, from: %s' % (self.type, self.value, self.MEASUREMENT_SI_UNIT[self.type], pretty_date)


class MeasurementNotification(models.Model):
    withings_user_id = models.BigIntegerField(name='userid')
    startdate = models.BigIntegerField()
    enddate = models.BigIntegerField()
    measurement_type = models.IntegerField(name='appli')
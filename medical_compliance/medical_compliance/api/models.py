from __future__ import unicode_literals

from django.db import models
from django.core.exceptions import ObjectDoesNotExist


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

    @staticmethod
    def get_measure_type_by_id(meas_id):
        for m in WithingsMeasurement.MEASURE_CHOICES:
            if m[0] == meas_id:
                return m[1]

        return None


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


class WeightMeasurement(models.Model):
    INPUT_SOURCES = (
        ('withings', 'withings'),
    )
    MEASUREMENT_UNITS = (
        ('kg', 'kg'),
    )

    user_id = models.BigIntegerField(name='user_id')
    input_source = models.CharField(max_length=20, choices=INPUT_SOURCES)
    measurement_unit = models.CharField(max_length=2, choices=MEASUREMENT_UNITS)
    timestamp = models.BigIntegerField()
    timezone = models.CharField(max_length=64)
    value = models.FloatField()

    @staticmethod
    def get_previous_weight_measures(reference_id, weights_count):
        try:
            retrieved_measurement = WeightMeasurement.objects.get(id=reference_id)
            last_weight_measurements = WeightMeasurement.objects.filter(timestamp__lte=retrieved_measurement.timestamp).order_by('-timestamp')[:weights_count]
            return last_weight_measurements
        except ObjectDoesNotExist:
            return []

    def __str__(self):
        return "{ user_id: %s, input_source: %s, measurement_unit: %s, timestamp: %s, timezone: %s, value: %s}" % \
            (self.user_id, self.input_source, self.measurement_unit, self.timestamp, self.timezone, self.value)
            
class HeartRateMeasurement(models.Model):
    INPUT_SOURCES = (
        ('cinch', 'cinch'),
        ('test', 'test')
    )
    MEASUREMENT_UNITS = (
        ('bpm', 'bpm'),
    )

    user_id = models.BigIntegerField(name='user_id')
    input_source = models.CharField(max_length=20, choices=INPUT_SOURCES)
    measurement_unit = models.CharField(max_length=3, choices=MEASUREMENT_UNITS)
    timestamp = models.BigIntegerField()
    timezone = models.CharField(max_length=64)
    value = models.FloatField()

    @staticmethod
    def get_previous_hr_measures(reference_id, hrm_count):
        try:
            retrieved_measurement = HeartRateMeasurement.objects.get(id=reference_id)
            last_hr_measurements = HeartRateMeasurement.objects.filter(timestamp__lte=retrieved_measurement.timestamp).order_by('-timestamp')[:hrm_count]
            return last_hr_measurements
        except ObjectDoesNotExist:
            return []
    
    def __str__(self):
        return "{ user_id: %s, input_source: %s, measurement_unit: %s, timestamp: %s, timezone: %s, value: %s}" % \
            (self.user_id, self.input_source, self.measurement_unit, self.timestamp, self.timezone, self.value)
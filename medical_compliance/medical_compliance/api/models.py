from __future__ import unicode_literals

from django.db import models


# Create your models here.
class MedicationPlan(models.Model):
    class Meta:
        db_table = 'medication_plan'

    plan_id = models.AutoField(primary_key=True)
    name = models.CharField(max_length=1000)
    daily_dose = models.IntegerField()

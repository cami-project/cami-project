using System;
namespace DSS.Rules.Library
{
    public class BloodPressureService
    {
        readonly IInform inform;

        public BloodPressureService(IInform inform)
        {
            this.inform = inform;
        }

        public void BloodRessureOK(Measurement measurement) 
        {
            //Console.WriteLine("bako");
            inform.Caregivers(measurement.user, measurement.measurement_type, "low", "Blood pressure is okay", "Blood pressure is okay", false);
        }

        public void InformCaregiverPrehypertension(Measurement measurement) 
        {
            inform.Caregivers(measurement.user, 
                              measurement.measurement_type, 
                              "high", "There is a risk of prehypertension" ,
                              string.Format("There is a risk of prehypertension {0} DBP/ {1} SBP", measurement.get("DBP"), measurement.get("SBP")));
            
        }

        public void InformCaregiverDanger(Measurement measurement) 
        {
            inform.Caregivers(measurement.user,
                  measurement.measurement_type,
                  "high", "Blood pressure is high",
                  string.Format("Blood pressure is dangerously high {0} DBP/ {1} SBP", measurement.get("DBP"), measurement.get("SBP")));
        }

    }
}

using System;
using DSS.Rules.Library.Domain;

namespace DSS.Rules.Library
{
    public class BloodPressureService
    {
        readonly IInform inform;

        public BloodPressureService(IInform inform)
        {
            this.inform = inform;
        }

        public void RiskOfPrehypertension(BloodPressureEvent e, bool in30MinAfterWakeup ) 
        {
            Console.WriteLine("Risk of prehypertension" + e.ToString() + in30MinAfterWakeup.ToString());

            //inform.Caregivers(measurement.user, 
                              //measurement.measurement_type, 
                              //"high", "There is a risk of prehypertension" ,
                              //string.Format("There is a risk of prehypertension {0} DBP/ {1} SBP", measurement.get("DBP"), measurement.get("SBP")));
        }

        public void BloodPessureOK(BloodPressureEvent e, bool in30MinAfterWakeup)
        {
            Console.WriteLine("Blood pressure is fineee" + e.ToString() + in30MinAfterWakeup.ToString());
        }

        public void BloodPressureIsDangerous(BloodPressureEvent e, bool in30MinAfterWakeup) 
        {

             Console.WriteLine("Blood pressure is dangerous" + e.ToString() + in30MinAfterWakeup.ToString());


            //inform.Caregivers(measurement.user,
                  //measurement.measurement_type,
                  //"high", "Blood pressure is high",
                  //string.Format("Blood pressure is dangerously high {0} DBP/ {1} SBP", measurement.get("DBP"), measurement.get("SBP")));
        }

    }
}

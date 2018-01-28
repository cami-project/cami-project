using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class NormalBloodPressure : Rule
    {
        BloodPressureService service = null;
        Measurement measurement = null;

        public override void Define()
        {
            //Check the domain object if it is of type BlOOD PRESSURE and if values are in normal range  
            When().Exists<Measurement>(measure => measure.isBloodPressure() && 
                                       measure.get("SBP") < 120 && 
                                       measure.get("DBP") < 80)
                  //nRule's way of saying propagate Measurement and service to the Then section
                  .Match(() => measurement)
                  .Match(() => service);

            //If all the rules from the When section are satisfied 
            Then().Do(ctx => service.BloodRessureOK(measurement));
        }
    }


    public class PrehypertensionBloodPressure : Rule
    {
        BloodPressureService service = null;
        Measurement measurement = null;

        public override void Define()
        {
            //Check the domain object if it is of type BlOOD PRESSURE and if values are in Prehypertension range  
            When().Exists<Measurement> (measure => measure.isBloodPressure() && 
                                       (measure.get("SBP") > 120 && measure.get("SBP") < 139) &&
                                       (measure.get("DBP") > 80) && measure.get("DBP") < 89)
                  //nRule's way of saying propagate Measurement and service to the Then section
                  .Match(() => measurement)
                  .Match(() => service);

            //If all the rules from the When section are satisfied 
            Then().Do(ctx => service.InformCaregiverPrehypertension(measurement));
        }
    }


    public class DangerBloodPressure : Rule
    {
        BloodPressureService service = null;
        Measurement measurement = null;

        public override void Define()
        {
            //Check the domain object if it is of type BlOOD PRESSURE and if values are in Danger range  
            When().Exists<Measurement>(measure => measure.isBloodPressure() && 
                                       measure.get("SBP") > 140 && 
                                       measure.get("DBP") > 90)

                  //nRule's way of saying propagate Measurement and service to the Then section
                  .Match(() => measurement)
                  .Match(() => service);
            
            //If all the rules from the When section are satisfied 
            Then().Do(ctx => service.InformCaregiverDanger(measurement));
        }
    }


}

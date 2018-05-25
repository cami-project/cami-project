using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class NormalBloodPressure : Rule
    {
        BloodPressureService service = null;
        Domain.BloodPressureEvent e = null;
        bool in30MinAfterWakeup = false;
        IActivityLog activityLog = null;

        public override void Define()
        {
            When().Exists<Domain.BloodPressureEvent>(x => x.DiastolicInRange(50, 80) && x.SystolicInRange(100, 120))
                  .Match(() => e)
                  .Match(() => service)
                  .Match(() => activityLog)
                  .Let(() => in30MinAfterWakeup, ()=> activityLog.TimeSince(e, ActivityType.WakeUp) < 30);
    
            Then().Do(ctx => service.BloodPessureOK(e, in30MinAfterWakeup));
        }
    }


    public class PrehypertensionBloodPressure : Rule
    {
        BloodPressureService service = null;
        Domain.BloodPressureEvent e = null;
        bool in30MinAfterWakeup = false;
        IActivityLog activityLog = null;


        public override void Define()
        {
            When().Exists<Domain.BloodPressureEvent> ( x=> x.SystolicInRange(121, 139) && x.DiastolicInRange(81, 89))
                  .Match(() => e)
                  .Match(() => service)
                  .Match(() => activityLog)
                  .Let(() => in30MinAfterWakeup, () => activityLog.TimeSince(e, ActivityType.WakeUp) < 30);
            
            Then().Do(ctx => service.RiskOfPrehypertension(e, in30MinAfterWakeup));
        }
    }


    public class DangerBloodPressure : Rule
    {
        BloodPressureService service = null;
        Domain.BloodPressureEvent e = null;
        bool in30MinAfterWakeup = false;
        IActivityLog activityLog = null;

        public override void Define()
        {
            When().Exists<Domain.BloodPressureEvent>(x => x.SystolicInRange(140, int.MaxValue) && x.DiastolicInRange(90, int.MaxValue)) 
                  .Match(() => e)
                  .Match(() => service)
                  .Match(() => activityLog)
                  .Let(() => in30MinAfterWakeup, () => activityLog.TimeSince(e, ActivityType.WakeUp) < 30);
            
            Then().Do(ctx => service.BloodPressureIsDangerous(e, in30MinAfterWakeup));
        }
    }


}

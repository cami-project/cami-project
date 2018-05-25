using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class FallRule : Rule
    {
        FallService service = null;
        Domain.FallEvent fall = null;
        IActivityLog activity = null;
        bool hasArrytmia = false;

        public override void Define()
        {

            When().Exists<Domain.FallEvent>()
                  .Match(() => fall)
                  .Match(() => service)
                  .Match(() => activity)
                  .Let(() => hasArrytmia, () => activity.EventOfTypeHappened(fall, ActivityType.LowPulse, 3));
            
            Then().Do(_ => service.InformCaregiverArrythmia(fall, hasArrytmia));
        }
    }

}

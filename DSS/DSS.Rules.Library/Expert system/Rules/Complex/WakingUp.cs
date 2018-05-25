using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class WakingUp : Rule
    {
        IActivityLog activityLog = null;
        ITimeService timeService = null;
        LocationChange locationChange = null;
        MotionService motionService = null;

        public override void Define()
        {
            When().Match(() => locationChange)
                  .Match(() => motionService)
                  .Exists<ITimeService>(x => x.HappenedInMorning(locationChange))
                  .Exists<IActivityLog>(x => x.IsAssumedState(locationChange, AssumedState.Sleeping));
            
            Then().Do(_ => motionService.Wakeup(locationChange));
        }
    }
}

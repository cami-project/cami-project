using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class Sleeping : Rule
    {
        LocationChange locationChange = null;
        MotionService service;

        public override void Define()
        {
            When().Exists<LocationChange>(locationTimeSpent => locationTimeSpent.ToBedroom())
                  .Match(() => service)
                  .Match(() => locationChange);

            Then().Do(ctx => service.SheeduleSleepingCheck(locationChange, 15));
        }
    }

    public class CheckSleeping : Rule
    {
        IActivityLog activityLog = null;
        SheduledEvent sheduledEvent = null;
        MotionService motionService;

        public override void Define()
        {
            When().Exists<SheduledEvent>(sheduledEvent => sheduledEvent.isSleepingCheck())
                  .Match(() => sheduledEvent)
                  .Match(() => activityLog)
                  .Match(() => motionService)
                  .Exists<IActivityLog>(activityLog => activityLog.DidNotHappenAfter(sheduledEvent.Owner, ActivityType.ShedulingSleepingCheck, ActivityType.Movement));

            Then().Do(_ => motionService.MightBeSleeping(sheduledEvent));
        }
    }
}

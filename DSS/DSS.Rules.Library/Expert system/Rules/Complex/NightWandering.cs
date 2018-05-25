using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class NightWandering : Rule
    {
        LocationChange locationChange = null;
        SuspiciousBehaviour suspiciousBehaviour = null;
        IActivityLog activityLog = null;
        ITimeService timeService = null;

        //TODO: Find a way not to update assumption before this is processed
        public override void Define()
        {
            When().Match(() => locationChange)
                  .Match(() => activityLog)
                  .Match(() => suspiciousBehaviour)
                  .Exists<ITimeService>(timeService => timeService.HappenedAtNight(locationChange))
                  .Exists<IActivityLog>(activityLog => activityLog.IsAssumedState(locationChange.Owner,AssumedState.Sleeping));

            Then().Do(ctx => suspiciousBehaviour.MightBeNightWandering(locationChange));
        }
    }

    public class CheckForNightWandering : Rule
    {

        SheduledEvent sheduledEvent = null;
        SuspiciousBehaviour suspiciousBehaviour = null;

        public override void Define()
        {
            When().Match(() => sheduledEvent)
                  .Match(() => suspiciousBehaviour)  //This can cause a bug since it check the time it was created
                  .Exists<SheduledEvent>(sheduledEvent => sheduledEvent.Is(SheduleService.Type.CheckForNightWandering))
                  .Exists<ITimeService>(timeService => timeService.HappenedAtNight(sheduledEvent))
                  .Exists<IActivityLog>(activityLog => activityLog.DidNotHappenAfter(sheduledEvent.Owner, ActivityType.MightBeNightWandering, ActivityType.MightBeSleeping));

            Then().Do(ctx => suspiciousBehaviour.NightWanderingConfirmed(sheduledEvent));
        }
    }
}

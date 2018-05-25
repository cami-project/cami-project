using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class NoMovementAfterFallDetected: Rule
    {
        SheduledEvent e;
        IActivityLog activityLog;
        FallService service;

        public override void Define()
        {
            When().Exists<SheduledEvent>(e => e.Type == SheduleService.Type.CheckMovementAfterFall)
                  .Match(() => e)
                  .Exists<IActivityLog>(activityLog => activityLog.EventOfTypeHappenedAfter(e, ActivityType.Fall, ActivityType.Movement))
                  .Match(() => activityLog)
                  .Exists<FallService>()
                  .Match(() => service);

            Then().Do(ctx => service.InformCaregiverOfMovementAfterFall());
        }

    }
}

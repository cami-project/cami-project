using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class MorningWeightReminder: Rule
    {
        SheduledEvent sheduledEvent = null;
        IActivityLog activity = null;
        ReminderService service = null;

        public override void Define()
        {

            When().Exists<SheduledEvent>(sheduledEvent => sheduledEvent.Is(SheduleService.Type.MorningWeightReminder))
                  .Match(() => sheduledEvent)
                  .Match(() => service)
                  .Exists<IActivityLog>(activity => activity.DidNotHappenAfter(sheduledEvent.Owner, ActivityType.WakeUp, ActivityType.WeightMeasured));

            Then().Do(ctx => service.SendWeightReminder(sheduledEvent));
        }
    }
}

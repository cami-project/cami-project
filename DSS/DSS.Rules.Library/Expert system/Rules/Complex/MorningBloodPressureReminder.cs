using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class MorningBloodPressureReminder : Rule
    {
        SheduledEvent sheduledEvent;
        LocationChange location;
        ReminderService service;

        public override void Define()
        {
            When().Exists<SheduledEvent>(sheduledEvent => sheduledEvent.Is(SheduleService.Type.MorningBloodPressureReminder))
                .Match(() => sheduledEvent)
                .Match(() => service)
                .Exists<IActivityLog>(activity => activity.DidNotHappenAfter(sheduledEvent, ActivityType.WakeUp, ActivityType.BloodPressureMeasured));

            Then().Do(ctx => service.SendMorningBloodPressureReminder(sheduledEvent));
        }
    }
}

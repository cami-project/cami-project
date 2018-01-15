using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class ReminderSentRule: Rule
    {
        ReminderService service = null;
        Event reminder = null;

        public override void Define()
        {
            When().Exists<Event>(e => e.isReminderSent())
                  .Match(() => service)
                  .Match(() => reminder);

             Then().Do(ctx => SheduleService.In( 1,()=> service.CheckIfAcknowledged(reminder), reminder.getUserURI()));
             Then().Do(ctx => service.Register(reminder) );
        }
    }
}

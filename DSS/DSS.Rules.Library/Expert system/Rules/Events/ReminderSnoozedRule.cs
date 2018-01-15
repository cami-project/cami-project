using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class ReminderSnoozedRule: Rule
    {
        ReminderService service = null;
        Event reminder = null;

        public override void Define()
        {
            When().Exists<Event>(reminder => reminder.isReminderSnoozed())
                  .Match(() => service)
                  .Match(() => reminder);


            Then().Do(ctx => service.Snoozed(reminder));
        }
    }
}

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
            //Check the sheduled event if it is of type REMINDER SNOOZED
            When().Exists<Event>(reminder => reminder.isReminderSnoozed())
                  //nRule's way of saying propagate FALL and service to the Then section
                  .Match(() => service)
                  .Match(() => reminder);

            //If all the rules from the When section are satisfied 
            Then().Do(ctx => service.Snoozed(reminder));
        }
    }
}

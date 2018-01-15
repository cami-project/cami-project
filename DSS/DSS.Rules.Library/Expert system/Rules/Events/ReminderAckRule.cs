using System;
using NRules.Fluent.Dsl;


namespace DSS.Rules.Library
{
    public class ReminderAckRule : Rule
    {
        ReminderService service = null;
        Event reminder = null;

        public override void Define()
        {
            When().Exists<Event>(reminder => reminder.isReminderACK())
                  .Match(() => service)
                  .Match(() => reminder);
            

            Then().Do(ctx => service.Acknowledge(reminder));
            Then().Do(ctx => service.CheckAckInDB(reminder));
        }
    }
}

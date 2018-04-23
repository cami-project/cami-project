using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class MorningBloodPressureReminder : Rule
    {
        
        LocationChange location;
        ReminderService service;

        public override void Define()
        {
            When().Exists<LocationChange>(location => location.FromTo("BATHROOM", "KITCHEN") &&
                                         !InMemoryDB.WeightReminderSent(location.ID))
                 .Match(() => location)
                 .Exists<ReminderService>()
                 .Match(() => service);

            Then().Do(ctx => service.SendMorningBloodPressureReminder(location.ID));
        }
    }
}

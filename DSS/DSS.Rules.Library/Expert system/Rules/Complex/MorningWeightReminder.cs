using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class MorningWeightReminder: Rule
    {
        LocationChange location;
        ReminderService service;

        public override void Define()
        {

            When().Exists<LocationChange>(location => location.FromTo("BATHROOM", "KITCHEN") && 
                                          !InMemoryDB.WeightReminderSent(location.Owner))
                  .Match(() => location)
                  .Exists<ReminderService>()
                  .Match(() => service);

            Then().Do(ctx => service.SendWeightReminder(location.Owner));
        }
    }
}

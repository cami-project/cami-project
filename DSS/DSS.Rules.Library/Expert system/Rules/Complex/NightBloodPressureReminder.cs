using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class NightBloodPressureReminder : Rule
    {
        LocationTimeSpent location;
        ReminderService service;

        public override void Define()
        {
            When().Exists<LocationTimeSpent>(location => location.Is("KITCHEN", 10) &&
                                             !InMemoryDB.NightBPSeminderSent(location.Owner) && 
                                             TimeService.isNight())
                 .Match(() => location)
                 .Exists<ReminderService>()
                 .Match(() => service);

            Then().Do(ctx => service.SendNightBloodPressureReminder(location.Owner));
        }
    }
}

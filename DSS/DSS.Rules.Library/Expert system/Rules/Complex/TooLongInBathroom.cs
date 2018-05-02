using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class TooLongInBathroom : Rule
    {
        LocationTimeSpent locationTimeSpent;
        SuspiciousBehaviour service;

        public override void Define()
        {
            When().Exists<LocationTimeSpent>(locationTimeSpent => locationTimeSpent.Is("BATHROOM", 1))
                  .Match(() => locationTimeSpent)
                  .Exists<SuspiciousBehaviour>()
                  .Match(() => service);

            Then().Do(ctx => service.ToLongInBathroom(locationTimeSpent));
        }
    }
}

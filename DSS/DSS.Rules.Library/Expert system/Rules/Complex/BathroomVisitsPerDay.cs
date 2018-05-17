using System;
using NRules.Fluent.Dsl;
using DSS.RMQ;

namespace DSS.Rules.Library
{
    public class BathroomVisitsPerDay: Rule
    {
        BathroomVisitsTwoDays visits;
        BathroomVisitService service;

        public override void Define()
        {
            When().Exists<BathroomVisitsTwoDays>(visits => visits.isValid() && visits.isOverTrehshold(-2))
                  .Match(() => visits)
                  .Exists<BathroomVisitService>()
                  .Match(() => service);

            Then().Do(ctx => service.InformCaregiverDayDifference(visits));
        }
    }
}

using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class BathroomVisitsPerWeek: Rule
    {
        BathroomVisitsWeek visits;
        BathroomVisitService service;

        public override void Define()
        {
            When().Exists<BathroomVisitsWeek>(visits => visits.isValid() && visits.tresholdFromAverage(-2))
                  .Match(() => visits)
                  .Exists<BathroomVisitService>()
                  .Match(() => service);

            Then().Do(ctx => service.InformCaregiverWeekAvgAbnormal(visits));
        }
    }
}

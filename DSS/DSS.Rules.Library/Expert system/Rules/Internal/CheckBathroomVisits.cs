using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class CheckBathroomVisits : Rule
    {
        SheduledEvent sheduledEvent = null;
        BathroomVisitService service = null;

        public override void Define()
        {
            When().Match<SheduledEvent>(sheduledEvent => sheduledEvent.isNewDay())
                  .Match(() => sheduledEvent)
                  .Exists<BathroomVisitService>()
                  .Match(() => service);

            //TODO: remove hardcoded value
            Then().Do(_ => service.Check(2));
        }
    }
}

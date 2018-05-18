using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class HighStepsCount: Rule
    {
        StepsService stepsService = null;
        SheduledEvent sheduledEvent = null;

        public override void Define()
        {
            When().Match<SheduledEvent>(sheduledEvent => sheduledEvent.isStepAnalisys())
                  .Match(() => sheduledEvent)
                  .Match<StepsService>(stepsService => stepsService.stepsCountBiggerThan(2000, sheduledEvent.Owner))
                  .Match(() => stepsService);

            Then().Do(_ => stepsService.InformHighSteps(sheduledEvent.Owner));
        }
    }
}

using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class MediumStepsCount: Rule
    {
        StepsService stepsService;
        SheduledEvent sheduledEvent;

        public override void Define()
        {
            When().Match<SheduledEvent>(sheduledEvent => sheduledEvent.isStepAnalisys())
                  .Match(() => sheduledEvent)
                  .Match<StepsService>(stepsService => stepsService.stepsCountBetween(1000, 2000, sheduledEvent.Owner))
                  .Match(() => stepsService);

            Then().Do(_ => stepsService.InformOfMediumSteps(sheduledEvent.Owner));
        }
    }
}

using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class LowStepsCount: Rule
    {
        StepsService stepsService;
        SheduledEvent sheduledEvent;

        public override void Define()
        {
            When().Match<SheduledEvent>(sheduledEvent => sheduledEvent.isStepAnalisys())
			      .Match(() => sheduledEvent)
                  .Match<StepsService>(stepsService => stepsService.stepsCountLessThan(1000, sheduledEvent.user))
                  .Match(() => stepsService);

            Then().Do(_ => stepsService.InformOfLowSteps(sheduledEvent.user));
        }
    }
}

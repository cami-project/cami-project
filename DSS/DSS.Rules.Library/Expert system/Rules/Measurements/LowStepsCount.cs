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
            //Check the sheduled event if it is of type STEP ANALISYS
            When().Match<SheduledEvent>(sheduledEvent => sheduledEvent.isStepAnalisys())
                  //nRule's way of saying propagate sheduled event to the Then section
			      .Match(() => sheduledEvent)
                  //Aditionl check to the number of steps
                  .Match<StepsService>(stepsService => stepsService.stepsCountLessThan(1000, sheduledEvent.Owner))
                  //nRule's way of saying propagate the step service to the Then section
                  .Match(() => stepsService);

            //If all the rules from the When section are satisfied   
            Then().Do(_ => stepsService.InformOfLowSteps(sheduledEvent.Owner));
        }
    }
}

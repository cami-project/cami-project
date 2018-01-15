using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class AnaliseStepsOncePerDay : Rule
    {

        public override void Define()
        {
            When().Match<SheduledEvent>(x => x.isNewDay());
            Then().Do(_ => SheduleService.OncePreDayAt(19, 0, SheduleService.Type.Steps));
        }
    }
}

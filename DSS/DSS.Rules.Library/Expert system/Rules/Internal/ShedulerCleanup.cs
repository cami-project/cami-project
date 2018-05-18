using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class ShedulerCleanup : Rule
    {
        SheduledEvent sheduledEvent = null;

        public override void Define()
        {
            When().Match<SheduledEvent>(sheduledEvent => sheduledEvent.isNewDay())
                  .Match(() => sheduledEvent);
                   Then().Do(_ => InMemoryDB.CleanHistory());
        }
    }
}

using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class FallRule: Rule
    {
        FallService service = null;
        Event fall = null;

        public override void Define()
        {
            When().Exists<Event>(fall => fall.isFall())
                  .Match(() => fall)
                  .Match(() => service);

            Then().Do(ctx => service.InformCaregiver(fall));
        }
    }
}

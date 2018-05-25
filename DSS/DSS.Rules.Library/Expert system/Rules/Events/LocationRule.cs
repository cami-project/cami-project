using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class LocationRule : Rule
    {

            MotionService service = null;
            Domain.MotionEvent motion = null;

            public override void Define()
            {
                    When().Match(() => motion)
                          .Match(() => service);

                    Then().Do(ctx => service.ChangeState(motion));
            }

    }
}

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

            //Check the domain object if it is of type FALL
            When().Exists<Event>(fall => fall.isFall())
                  //nRule's way of saying propagate FALL and service to the Then section
                  .Match(() => fall)
                  .Match(() => service);

            //If all the rules from the When section are satisfied 
            Then().Do(ctx => service.InformCaregiver(fall));
        }
    }
}

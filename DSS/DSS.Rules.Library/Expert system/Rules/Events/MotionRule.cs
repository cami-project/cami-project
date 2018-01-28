using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class MotionRule: Rule
    {
        MotionService service = null;
        Event motion = null;

        public override void Define()
        {

            //Check the domain object if it is of type MOTION ALARM
            When().Exists<Event>(motion => motion.isMotionAlarm())
                  //nRule's way of saying propagate MOTION ALARM EVENT and service to the Then section
                  .Match(() => motion)
                  //Aditionl check to make sure it's morning 
                  .Exists<MotionService>(service => service.isMorning(motion))
                  //nRule's way of saying propagate Motion service  to the Then section
                  .Match(() => service);

            //If all the rules from the When section are satisfied 
            Then().Do(ctx => service.SendBloodPreasureMeasurementReminder(motion));
        }
    }
}

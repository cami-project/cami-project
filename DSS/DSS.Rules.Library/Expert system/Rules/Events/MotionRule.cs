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
            When().Exists<Event>(motion => motion.isMotionAlarm())
                  .Match(() => motion)
                  .Exists<MotionService>(service => service.isMorning(motion))
                  .Match(() => service);

            Then().Do(ctx => service.SendBloodPreasureMeasurementReminder(motion));
        }
    }
}

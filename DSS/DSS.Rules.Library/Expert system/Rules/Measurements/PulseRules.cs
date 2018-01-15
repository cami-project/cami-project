using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class PulseLowRule : Rule
    {
        PulseService service = null;
        Measurement measurement = null;

        public override void Define()
        {

            When().Exists<Measurement>(measure => measure.isPulse() && measure.isLessThen(30))
                  .Match(()=> measurement)
                  .Match(() => service);

            Then().Do(ctx => service.InformOfLowPulse(measurement));
			Then().Do(ctx => service.Save(measurement));
        }
    }
    public class PulseMidLowRule : Rule
    {
        PulseService service = null;
        Measurement measurement = null;

        public override void Define()
        {
            When().Exists<Measurement>(measure => measure.isPulse() && measure.isBetween(30, 60) && !measure.isNight())
                  .Match(() => measurement)
                  .Match(() => service);
            
			Then().Do(ctx => service.InformOfMidLowPulse(measurement));
            Then().Do(ctx => service.Save(measurement));
        }
    }


    public class PulseMidHighRule : Rule
    {
        PulseService service = null;
        Measurement measurement = null;

        public override void Define()
        {
            When().Exists<Measurement>(measure => measure.isPulse() && measure.isBetween(100, 120))
                  .Match(() => measurement)
                  .Match(() => service);

            Then().Do(ctx => service.InformOfMidHighPulse(measurement));
			Then().Do(ctx => service.Save(measurement));
        }
    }

    public class PulseHighRule : Rule
    {
        PulseService service = null;
        Measurement measurement = null;

        public override void Define()
        {
            When().Exists<Measurement>(measure => measure.isPulse() && measure.isBiggerThen(120))
                  .Match(() => measurement)
                  .Match(() => service);

            Then().Do(ctx => service.InformOfHighPulse(measurement));
			Then().Do(ctx => service.Save(measurement));
        }
    }
}
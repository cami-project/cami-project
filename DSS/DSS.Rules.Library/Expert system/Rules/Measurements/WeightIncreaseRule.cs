using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{

        public class WeightIncreaseRule : Rule
        {
            WeightService service = null;
            Measurement measure = null;

            public override void Define()
            {
                 When().Exists<Measurement>(measure => measure.isWeight())
                       .Match(() => measure)
                       .Match(() => service, service => service.IsBiggerThenPreviousBy(measure, 2));

                 Then().Do(ctx => service.WeightIncrease(measure));
                 Then().Do(ctx => service.Save(measure));
            }
        }
 }
    


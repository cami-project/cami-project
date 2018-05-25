using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{

    //public class WeightDropRule : Rule
    //{
    //    WeightService service = null;
    //    Measurement measure = null;

    //    public override void Define()
    //    {
    //        When().Exists<Measurement>(measure => measure.isWeight())
    //              .Match(() => measure)
    //              .Match(() => service, service => service.IsLessThenPreviousBy(measure,2));
            
    //        Then().Do(ctx => service.WeightDrop(measure));
    //        Then().Do(ctx => service.Save(measure));
    //    }
    //}
}

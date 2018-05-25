using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{

        //public class WeightIncreaseRule : Rule
        //{
        //    WeightService service = null;
        //    Measurement measure = null;

        //    public override void Define()
        //    {
        //         When().Exists<Measurement>(measure => measure.isWeight())
        //               .Match(() => measure)
        //               .Match(() => service, service => service.IsBiggerThenPreviousBy(measure, 2));
            
        //         Then().Do(ctx => measure.SetOK(InMemoryDB.WeightReminderSentBeforeMin(measure.user, 30)));
        //         Then().Do(ctx => service.WeightIncrease(measure));
        //         Then().Do(ctx => service.Save(measure));
        //    }
        //}


    public class WeightMeasured : Rule
    {

        Domain.WeightEvent weight = null;
        WeightService weightService = null;
        IActivityLog activityLog = null;
        bool lessThan30AfterWakeup = false;

        public override void Define()
        {
            When().Match(() => weight)
                  .Match(() => activityLog)
                  .Match(() => weightService)
                  .Let(() => lessThan30AfterWakeup, () => activityLog.TimeSince(weight, ActivityType.WakeUp) < 30);


            Then().Do(_ => weightService.Save(weight, lessThan30AfterWakeup));
 }
    }
}
    


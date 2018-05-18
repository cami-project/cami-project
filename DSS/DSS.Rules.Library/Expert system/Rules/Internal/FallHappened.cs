using NRules.Fluent.Dsl; 

namespace DSS.Rules.Library
{
    public class FallHappened : Rule
    {
        Activity activity = null;
        FallService service = null;

        public override void Define()
        {
            When().Exists<Activity>(activity => activity.IsType(ActivityType.Fall))
                  .Match(() => activity)
                  .Match(() => service);


            Then().Do(_ => service.SheduleCheckForMovementAfter(activity.Owner ,2));
        }
    }
}

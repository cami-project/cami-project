using System;
using NRules.Fluent.Dsl;

namespace DSS.Rules.Library
{
    public class LeftHouse : Rule
    {
        LocationChange locationChange = null;
        MotionService motionService = null;

        public override void Define()
        {
            When().Match(() => locationChange)
                  .Match(() => motionService)  //This can cause a bug since it check the time it was created
                  .Exists<LocationChange>(locationChange => locationChange.ToHallway());

            Then().Do(ctx => motionService.SheduleHouseLeftCheck(locationChange, 3));
        }
    }


    public class ConfirmLeftHouse : Rule 
    {
        SheduledEvent sheduledEvent = null;
        MotionService motionService = null;
        IActivityLog activityLog = null;

        public override void Define()
        {
            When().Match(() => sheduledEvent)
                  .Match(() => motionService)  
                  .Exists<SheduledEvent>(sheduledEvent => sheduledEvent.Is(SheduleService.Type.CheckIfLeftHouse))
                  .Exists<IActivityLog>(activityLog => activityLog.DidNotHappenAfter(sheduledEvent.Owner, ActivityType.ShedulingLeftHouseCheck, ActivityType.Movement));


            Then().Do(ctx => motionService.MightLeftHouse(sheduledEvent));
      
        }



    }
}

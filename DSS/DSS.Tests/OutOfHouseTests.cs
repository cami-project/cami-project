using DSS.RMQ;
using DSS.Rules.Library;
using NUnit.Framework;
using System;


namespace DSS.Tests
{
    [TestFixture()]
    public class OutOfHouseTests
    {
        [Test()]
        public void ShedulingOutOfHouseCheck()
        {
            var activityLog = new ActivityLog();
            var usr = "/api/v1/user/2/";

            activityLog.ChangeAssumedState(usr, AssumedState.Awake);

            var inform = new MockInform(new MockStoreAPI(), null, activityLog);

            var ruleHandler = new RuleHandler(inform, activityLog);
            var motion = new LocationChange(new DSS.Rules.Library.Domain.MotionEvent() { Owner = usr, Location= "HALLWAY" }, "KITCHEN");
            ruleHandler.HandleLocationChange(motion);

           // Assert.IsTrue(inform.ActivityLog.GetAssumedState(usr) == AssumedState.Awake);
        }

        [Test()]
        public void SheduledOutOfHouseCheck_Positive()
        {
            var activityLog = new ActivityLog();
            var usr = "/api/v1/user/2/";

            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Movement detected", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.ShedulingLeftHouseCheck, "Sheduling left house check", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.LowPulse, "Movement detected", "Test"));

            var inform = new MockInform(new MockStoreAPI(), null, activityLog);
            var ruleHandler = new RuleHandler(inform, activityLog);

            ruleHandler.HandleSheduled(new SheduledEvent(usr, SheduleService.Type.CheckIfLeftHouse, DateTime.UtcNow));

            Assert.IsTrue(activityLog.GetAssumedState(usr) == AssumedState.Outside);
         
        }
    }
}

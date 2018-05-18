using NUnit.Framework;
using System;

using DSS.Rules.Library;
using DSS.RMQ;

namespace DSS.Tests
{
    [TestFixture()]
    public class Test
    {
        [Test()]
        public void BathroomVisitsTwoDaysInRow()
        {
            var ruleHandler = new RuleHandler(new ActivityLog());

            var inform = new Inform(new MockStoreAPI(), null);
            var service = new BathroomVisitService(inform, ruleHandler.BathroomVisitsDayHandler, ruleHandler.BathroomVisitsWeekHandler);

            service.Check(2);
        }

        [Test()]
        public void ActivityLog_FallLast_Test()
        {

            var activityLog = new ActivityLog();
            var usr = "/api/v1/user/2/";

            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Fall, "Description: A fall", "Test"));


            Assert.IsFalse( activityLog.EventOfTypeHappenedAfter(usr, ActivityType.Fall, ActivityType.Movement));
        }

        [Test()]
        public void ActivityLog_MovementAfterFall_Test()
        {

            var activityLog = new ActivityLog();
            var usr = "/api/v1/user/2/";

            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Fall, "Description: A fall", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Description: A movement", "Test"));

            Assert.IsTrue(activityLog.EventOfTypeHappenedAfter(usr, ActivityType.Fall, ActivityType.Movement));
        }


        [Test()]
        public void ActivityLog_NoMovementAfter_WithMultipleFalls_Test()
        {

            var activityLog = new ActivityLog();
            var usr = "/api/v1/user/2/";

            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Fall, "Description: A fall", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Description: A movement", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Fall, "Description: A fall", "Test"));

            Assert.IsFalse(activityLog.EventOfTypeHappenedAfter(usr, ActivityType.Fall, ActivityType.Movement));
        }

        [Test()]
        public void MovementAfterFall_RuleEngine_Test()
        {

            var activityLog = new ActivityLog();
            var usr = "/api/v1/user/2/";

            var ruleHandler = new RuleHandler(activityLog);


            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Fall, "Description: A fall", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Description: A movement", "Test"));

            ruleHandler.HandleSheduled(new SheduledEvent(usr,SheduleService.Type.CheckMovementAfterFall, DateTime.Now));

            //Assert.IsTrue(activityLog.EventOfTypeHappenedAfter(usr, ActivityType.Fall, ActivityType.Movement));
        }


    }
}

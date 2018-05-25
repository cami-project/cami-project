using NUnit.Framework;
using System;

using DSS.Rules.Library;
using DSS.RMQ;

namespace DSS.Tests
{
    [TestFixture()]
    public class Test
    {

        private IOwner usr;

        [SetUp]
        public void SetUp()
        {
            usr = new MockOwner("/api/v1/user/2/");

        }


        [Test()]
        public void BathroomVisitsTwoDaysInRow()
        {

            var inform = new MockInform(new MockStoreAPI(), null, null);

            var ruleHandler = new RuleHandler(inform ,new ActivityLog());

            var service = new BathroomVisitService(inform, ruleHandler.BathroomVisitsDayHandler, ruleHandler.BathroomVisitsWeekHandler);

            service.Check(2);
        }

        [Test()]
        public void ActivityLog_FallLast()
        {

            var activityLog = new ActivityLog();

            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Fall, "Description: A fall", "Test"));


            Assert.IsFalse( activityLog.EventOfTypeHappenedAfter(usr, ActivityType.Fall, ActivityType.Movement));
        }
        [Test()]
        public void ActivityLog_EventOfTypeHappened()
        {
            
            var activityLog = new ActivityLog();

            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.LowPulse, "Low pulse detected", "Test"));

            Assert.IsTrue(activityLog.EventOfTypeHappened(usr, ActivityType.LowPulse, 1));
        }

        [Test()]
        public void ActivityLog_EventOfTypeHappened_Timeout()
        {
            
            var activityLog = new ActivityLog();

            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            var activity = new Activity(usr, ActivityType.LowPulse, "Low pulse detected", "Test");

            activity.Timestamp = activity.Timestamp.AddMinutes(-10);

            Console.WriteLine(activity);

            activityLog.Log(activity);

            Assert.IsFalse(activityLog.EventOfTypeHappened(usr, ActivityType.LowPulse, 5));
        }

        [Test()]
        public void ActivityLog_EventOfTypeHappened_NotExist()
        {

            var activityLog = new ActivityLog();


            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Fall, "Fall detected", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Movement detected", "Test"));

            Assert.IsFalse(activityLog.EventOfTypeHappened(usr, ActivityType.LowPulse,1));
        }



        [Test()]
        public void ActivityLog_MovementAfterFall()
        {

            var activityLog = new ActivityLog();

            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Fall, "Description: A fall", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Description: A movement", "Test"));

            Assert.IsTrue(activityLog.EventOfTypeHappenedAfter(usr, ActivityType.Fall, ActivityType.Movement));
        }


        [Test()]
        public void ActivityLog_NoMovementAfter_WithMultipleFalls()
        {

            var activityLog = new ActivityLog();

            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Fall, "Description: A fall", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Description: A movement", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Fall, "Description: A fall", "Test"));

            Assert.IsFalse(activityLog.EventOfTypeHappenedAfter(usr, ActivityType.Fall, ActivityType.Movement));
        }

        [Test()]
        public void MovementAfterFall_RuleEngine()
        {

            var activityLog = new ActivityLog();

            var inform = new MockInform(null, null, activityLog);

            var ruleHandler = new RuleHandler(inform ,activityLog);


            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Fall, "Description: A fall", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Description: A movement", "Test"));

            ruleHandler.HandleSheduled(new SheduledEvent(usr,SheduleService.Type.CheckMovementAfterFall, DateTime.Now));

            //Assert.IsTrue(activityLog.EventOfTypeHappenedAfter(usr, ActivityType.Fall, ActivityType.Movement));
        }

        [Test()]
        public void ActivityLog_EventOfTypeHappened_RuleEngine()
        {

            var activityLog = new ActivityLog();

            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.LowPulse, "Low pulse detected", "Test"));


            var inform = new MockInform(new MockStoreAPI(), null, activityLog);

            var ruleHandler = new RuleHandler(inform, activityLog);

            ruleHandler.HandleEvent(new Event() { content = new Content() { name = "fall" }, Owner = usr.Owner, Lang = "EN" });


            //Assert.IsTrue(activityLog.EventOfTypeHappened(usr, ActivityType.LowPulse));
        }

        [Test()]
        public void ActivityLog_EventOfTypeHappened_Not_RuleEngine()
        {

            var activityLog = new ActivityLog();

            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Movement detected", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Movement detected", "Test"));


            var inform = new MockInform(new MockStoreAPI(), null, activityLog);

            var ruleHandler = new RuleHandler(inform, activityLog);

            ruleHandler.HandleEvent(new Event() { content = new Content() { name = "fall" }, Owner = usr.Owner, Lang = "EN" });

            //Assert.IsTrue(activityLog.EventOfTypeHappened(usr, ActivityType.LowPulse));
        }



        [Test()]
        public void MovementAfterFall_RawFallEvent_RuleEngine()
        {

            var activityLog = new ActivityLog();

            var inform = new MockInform(null, null, activityLog);
            var ruleHandler = new RuleHandler(inform ,activityLog);


            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Fall, "Description: A fall", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Description: A movement", "Test"));


            activityLog.ActivityRuleHandler = ruleHandler.ActivityHandler;

            ruleHandler.HandleEvent(new Event() { content = new Content() { name = "fall" }, Owner = usr.Owner, Lang = "EN" });


           // ruleHandler.HandleSheduled(new SheduledEvent(usr, SheduleService.Type.CheckMovementAfterFall, DateTime.Now));

            //Assert.IsTrue(activityLog.EventOfTypeHappenedAfter(usr, ActivityType.Fall, ActivityType.Movement));
        }


    }
}

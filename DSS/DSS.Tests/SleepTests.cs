using DSS.RMQ;
using DSS.Rules.Library;
using NUnit.Framework;
using System;
namespace DSS.Tests
{
    [TestFixture()]
    public class SleepTests
    {

        private IOwner usr;

        [SetUp]
        public void SetUp()
        {
            usr = new MockOwner("/api/v1/user/2/");

        }


        [Test()]
        public void MightBeSleeping_Rule()
        {
            var activityLog = new ActivityLog();

            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Movement detected", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.ShedulingSleepingCheck, "Sheduling sleeping check", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.LowPulse, "Low pulse", "Test"));

            var inform = new MockInform(new MockStoreAPI(), null, activityLog);

            var ruleHandler = new RuleHandler(inform, activityLog);

            ruleHandler.HandleSheduled(new SheduledEvent(usr, SheduleService.Type.SleepCheck, DateTime.Now));


        }

        [Test()]
        public void MightBeSleeping_Interupted_Rule()
        {
            var activityLog = new ActivityLog();

            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Movement detected", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.ShedulingSleepingCheck, "Sheduling sleeping check", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Low pulse", "Test"));

            var inform = new MockInform(new MockStoreAPI(), null, activityLog);

            var ruleHandler = new RuleHandler(inform, activityLog);

            ruleHandler.HandleSheduled(new SheduledEvent(usr, SheduleService.Type.SleepCheck, DateTime.Now));

        }

        [Test()]
        public void WakingUpFromSleep_DuringNight_Rule()
        {
            var activityLog = new ActivityLog();

            activityLog.ChangeAssumedState(usr, AssumedState.Sleeping);

            var inform = new MockInform(new MockStoreAPI(), null, activityLog);

            var ruleHandler = new RuleHandler(inform, activityLog);
            var motion = new LocationChange(new DSS.Rules.Library.Domain.MotionEvent() { Owner = usr.Owner}, "BEDROOM");
            ruleHandler.HandleLocationChange(motion);

            Assert.IsTrue(inform.ActivityLog.GetAssumedState(usr) == AssumedState.Awake);
        }


        [Test()]
        public void NightWanderingConfirmedAfterAnHour()
        {
            var activityLog = new ActivityLog();

            activityLog.Log(new Activity(usr, ActivityType.Movement, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Movement detected", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.ShedulingSleepingCheck, "Sheduling sleeping check", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.MightBeSleeping, "Low pulse", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.MightBeNightWandering, "Low pulse", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Movement detected", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Fall, "Movement detected", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Movement detected", "Test"));

            var inform = new MockInform(new MockStoreAPI(), null, activityLog);
            var ruleHandler = new RuleHandler(inform, activityLog);

            ruleHandler.HandleSheduled(new SheduledEvent(usr, SheduleService.Type.CheckForNightWandering, DateTime.UtcNow));

            Assert.IsTrue(activityLog.GetLastActivityType(usr) == ActivityType.NightWanderingConfirmed);
        }
    }
}

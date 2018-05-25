using NUnit.Framework;
using DSS.Rules.Library;
using System;


namespace DSS.Tests
{
    [TestFixture()]
    public class MotionServiceTests
    {
        private IOwner usr;

        [SetUp]
        public void SetUp()
        {
            usr = new MockOwner("/api/v1/user/2/");
        }


        [Test()]
        public void ChangeState_addNewState()
        {
            
            var motionService = new MotionService(null, new MockRuleHandler(), null );

            motionService.ChangeState(new Rules.Library.Domain.MotionEvent() { Owner = usr.Owner, Timestamp = DateTime.UtcNow, Location = "BEDROOM" });
        
        }

        [Test()]
        public void ChangeState_addNewAndChange()
        {
            var motionService = new MotionService(null, new MockRuleHandler(), null);
            //var usr = "/api/v1/user/2/";

            motionService.ChangeState(new Rules.Library.Domain.MotionEvent() { Owner = usr.Owner, Timestamp = DateTime.UtcNow, Location = "KITCHEN" });
            motionService.ChangeState(new Rules.Library.Domain.MotionEvent() { Owner = usr.Owner, Timestamp = DateTime.UtcNow, Location = "BEDROOM" });
        }

        [Test()]
        public void ChangeState_sameLocation()
        {
            var motionService = new MotionService(null,new MockRuleHandler(), null);
            //var usr = "/api/v1/user/2/";

            motionService.ChangeState(new Rules.Library.Domain.MotionEvent() { Owner = usr.Owner, Timestamp = DateTime.UtcNow, Location = "BEDROOM" });
            motionService.ChangeState(new Rules.Library.Domain.MotionEvent() { Owner = usr.Owner, Timestamp = DateTime.UtcNow, Location = "BEDROOM" });
        }

        [Test()]
        public void SheduleSleepCheckForBedroom_RuleEngine()
        {

            var activityLog = new ActivityLog();
            //var usr = "/api/v1/user/2/";

            var inform = new MockInform(null, null, activityLog);

            var ruleHandler = new RuleHandler(inform, activityLog);
            activityLog.ActivityRuleHandler = ruleHandler.ActivityHandler;

            ruleHandler.HandleLocationChange(new LocationChange(usr, "KITCHEN", "BEDROOM"));
        }

        [Test()]
        public void Wakeup_RuleEngine()
        {
            var activityLog = new ActivityLog();
            //var usr = "/api/v1/user/2/";

            activityLog.ChangeAssumedState(usr, AssumedState.Sleeping);

            var inform = new MockInform(null, null, activityLog);
            var ruleHandler = new RuleHandler(inform, activityLog);
            activityLog.ActivityRuleHandler = ruleHandler.ActivityHandler;

            ruleHandler.HandleLocationChange(new LocationChange(usr, "KITCHEN", "BEDROOM"));

            Assert.IsTrue(activityLog.GetAssumedState(usr) == AssumedState.Awake);
        }


        [Test()]
        public void Wakeup_WeightReminder_RuleEngine()
        {
            var activityLog = new ActivityLog();
            //var usr = "/api/v1/user/2/";

            activityLog.ChangeAssumedState(usr, AssumedState.Awake);

            var inform = new MockInform(null, null, activityLog);
            var ruleHandler = new RuleHandler(inform, activityLog);
            activityLog.ActivityRuleHandler = ruleHandler.ActivityHandler;


            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.WakeUp, "Description: A wake up", "Test"));


            ruleHandler.HandleSheduled(new SheduledEvent(usr, SheduleService.Type.MorningWeightReminder, DateTime.UtcNow));


            Assert.IsTrue(activityLog.GetLastActivityType(usr) == ActivityType.MorningWeightReminderSent);

            //ruleHandler.HandleLocationChange(new LocationChange(usr, "KITCHEN", "BEDROOM"));
            //Assert.IsTrue(activityLog.GetAssumedState(usr) == AssumedState.Awake);
        }
        [Test()]
        public void Wakeup_WeightReminder_WeightMeasured_RuleEngine()
        {
            var activityLog = new ActivityLog();
            //var usr = "/api/v1/user/2/";

            activityLog.ChangeAssumedState(usr, AssumedState.Awake);

            var inform = new MockInform(null, null, activityLog);
            var ruleHandler = new RuleHandler(inform, activityLog);
            activityLog.ActivityRuleHandler = ruleHandler.ActivityHandler;


            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.WakeUp, "Description: A wake up", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Description: A wake up", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Description: A wake up", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.WeightMeasured, "Description: A wake up", "Test"));


            ruleHandler.HandleSheduled(new SheduledEvent(usr, SheduleService.Type.MorningWeightReminder, DateTime.UtcNow));

            //ruleHandler.HandleLocationChange(new LocationChange(usr, "KITCHEN", "BEDROOM"));
            //Assert.IsTrue(activityLog.GetAssumedState(usr) == AssumedState.Awake);
        }

    }
}

using DSS.RMQ;
using DSS.Rules.Library;
using NUnit.Framework;
using System;
namespace DSS.Tests
{
    [TestFixture()]
    public class Weight
    {

        private IOwner usr;

        [SetUp]
        public void SetUp()
        {
            usr = new MockOwner("/api/v1/user/2/");

        }


        [Test()]
        public void WeightAddedInLessThan30Min()
        {
            var activityLog = new ActivityLog();


            activityLog.Log(new Activity(usr, ActivityType.Movement, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.WakeUp, "Movement detected", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Fall, "Movement detected", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Movement detected", "Test"));

            var inform = new MockInform(new MockStoreAPI(), null, activityLog);
            var ruleHandler = new RuleHandler(inform, activityLog);

            ruleHandler.Handle(new DSS.Rules.Library.Domain.WeightEvent() { Owner = usr.Owner, Value = 10, PreviousValue = 7, Timestamp = DateTime.UtcNow });
            //Assert.IsTrue(activityLog.GetLastActivityType(usr) == ActivityType.NightWanderingConfirmed);
        }

        [Test()]
        public void WeightAddedInMoreThan30Min()
        {
            var activityLog = new ActivityLog();

            activityLog.Log(new Activity(usr, ActivityType.Movement, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.WakeUp, "Movement detected", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Fall, "Movement detected", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Movement detected", "Test"));

            var inform = new MockInform(new MockStoreAPI(), null, activityLog);
            var ruleHandler = new RuleHandler(inform, activityLog);

            ruleHandler.Handle(new DSS.Rules.Library.Domain.WeightEvent() { Owner = usr.Owner, Value = 10, PreviousValue = 7, Timestamp = DateTime.UtcNow.AddMinutes(59) });
            //Assert.IsTrue(activityLog.GetLastActivityType(usr) == ActivityType.NightWanderingConfirmed);

        }
    }
}

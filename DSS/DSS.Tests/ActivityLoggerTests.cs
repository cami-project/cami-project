using NUnit.Framework;
using System;
using DSS.Rules.Library;


namespace DSS.Tests
{
    [TestFixture()]
    public class ActivityLoggerTests
    {

        private IOwner usr;

        [SetUp]
        public void SetUp()
        {
            usr = new MockOwner("/api/v1/user/2/");

        }


        [Test()]
        public void DidNotHappenAfter()
        {

            var activityLog = new ActivityLog();

            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Fall, "Description: A fall", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.ShedulingSleepingCheck, "Description: A movement", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Fall, "Description: A fall", "Test"));

            Assert.IsTrue(activityLog.DidNotHappenAfter(usr, ActivityType.ShedulingSleepingCheck, ActivityType.Movement));
        }


        [Test()]
        public void DidNotHappenAfter_Happened()
        {
            
            var activityLog = new ActivityLog();

            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Fall, "Description: A fall", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.ShedulingSleepingCheck, "Description: A movement", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Description: A movement", "Test"));

            Assert.IsFalse(activityLog.DidNotHappenAfter(usr, ActivityType.ShedulingSleepingCheck, ActivityType.Movement));
        }


        [TestCase()]
        public void ChangeAssumedState()
        {
            var activityLog = new ActivityLog();
            activityLog.ChangeAssumedState(usr, AssumedState.Sleeping);

            Assert.IsTrue(activityLog.GetAssumedState(usr) == AssumedState.Sleeping);
        }
    }
}

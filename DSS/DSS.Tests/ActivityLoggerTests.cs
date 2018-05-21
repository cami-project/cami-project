using NUnit.Framework;
using System;
using DSS.Rules.Library;


namespace DSS.Tests
{
    [TestFixture()]
    public class ActivityLoggerTests
    {
        [Test()]
        public void DidNotHappenAfter()
        {

            var activityLog = new ActivityLog();
            var usr = "/api/v1/user/2/";

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
            var usr = "/api/v1/user/2/";

            activityLog.Log(new Activity(usr, ActivityType.Null, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Fall, "Description: A fall", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.ShedulingSleepingCheck, "Description: A movement", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Description: A movement", "Test"));

            Assert.IsFalse(activityLog.DidNotHappenAfter(usr, ActivityType.ShedulingSleepingCheck, ActivityType.Movement));
        }


        [TestCase()]
        public void ChangeAssumedState()
        {
            var usr = "/api/v1/user/2/";
            var activityLog = new ActivityLog();
            activityLog.ChangeAssumedState(usr, AssumedState.Speeping);

            Assert.IsTrue(activityLog.GetAssumedState(usr) == AssumedState.Speeping);
        }
    }
}

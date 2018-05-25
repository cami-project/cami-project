using System;
using DSS.RMQ;
using DSS.Rules.Library;
using NUnit.Framework;

namespace DSS.Tests
{
    [TestFixture]
    public class BloodPressure
    {
        [Test()]
        public void SendReminder()
        {
            var activityLog = new ActivityLog();
            var usr = "/api/v1/user/2/";

            activityLog.Log(new Activity(usr, ActivityType.WakeUp, "Empty activity", "Test"));
             activityLog.Log(new Activity(usr, ActivityType.Movement, "Movement detected", "Test"));

            var inform = new MockInform(new MockStoreAPI(), null, activityLog);
            var ruleHandler = new RuleHandler(inform, activityLog);

            ruleHandler.HandleSheduled(new SheduledEvent(usr, SheduleService.Type.MorningBloodPressureReminder, DateTime.UtcNow));
        } 

        [Test()]
        public void DontSendReminder()
        {
            var activityLog = new ActivityLog();
            var usr = "/api/v1/user/2/";

            activityLog.Log(new Activity(usr, ActivityType.WakeUp, "Empty activity", "Test"));
             activityLog.Log(new Activity(usr, ActivityType.Movement, "Movement detected", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.BloodPressureMeasured, "Movement detected", "Test"));

            var inform = new MockInform(new MockStoreAPI(), null, activityLog);
            var ruleHandler = new RuleHandler(inform, activityLog);

            ruleHandler.HandleSheduled(new SheduledEvent(usr, SheduleService.Type.MorningBloodPressureReminder, DateTime.UtcNow));
        }

        [Test()]
        public void NormalBloodPressure()
        {
            var activityLog = new ActivityLog();
            var usr = "/api/v1/user/2/";

            activityLog.Log(new Activity(usr, ActivityType.WakeUp, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Movement detected", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.BloodPressureMeasured, "Movement detected", "Test"));

            var inform = new MockInform(new MockStoreAPI(), null, activityLog);
            var ruleHandler = new RuleHandler(inform, activityLog);

            ruleHandler.Handle(new DSS.Rules.Library.Domain.BloodPressureEvent() { 
                Owner = usr,
                Diastolic = 79, 
                Systolic = 110, 
                Timestamp = DateTime.UtcNow 
            });
        }

        [Test()]
        public void NormalBloodPressure_Late()
        {
            var activityLog = new ActivityLog();
            var usr = "/api/v1/user/2/";

            activityLog.Log(new Activity(usr, ActivityType.WakeUp, "Empty activity", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.Movement, "Movement detected", "Test"));
            activityLog.Log(new Activity(usr, ActivityType.BloodPressureMeasured, "Movement detected", "Test"));

            var inform = new MockInform(new MockStoreAPI(), null, activityLog);
            var ruleHandler = new RuleHandler(inform, activityLog);

            ruleHandler.Handle(new DSS.Rules.Library.Domain.BloodPressureEvent()
            {
                Owner = usr,
                Diastolic = 79,
                Systolic = 110,
                Timestamp = DateTime.UtcNow.AddMinutes(50)
            });
        }


    }
}
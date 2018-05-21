using NUnit.Framework;
using DSS.Rules.Library;
using System;


namespace DSS.Tests
{
    [TestFixture()]
    public class MotionServiceTests
    {
        [Test()]
        public void ChangeState_addNewState()
        {
            var motionService = new MotionService(null, null, (change)=> Console.WriteLine(change), null );
            var usr = "/api/v1/user/2/";

            motionService.ChangeState(new Rules.Library.Domain.MotionEvent() { Owner = usr, Timestamp = DateTime.UtcNow, Location = "BEDROOM" });
        
        }

        [Test()]
        public void ChangeState_addNewAndChange()
        {
            var motionService = new MotionService(null, null, (change) => Console.WriteLine(change), null);
            var usr = "/api/v1/user/2/";

            motionService.ChangeState(new Rules.Library.Domain.MotionEvent() { Owner = usr, Timestamp = DateTime.UtcNow, Location = "KITCHEN" });
            motionService.ChangeState(new Rules.Library.Domain.MotionEvent() { Owner = usr, Timestamp = DateTime.UtcNow, Location = "BEDROOM" });
        }

        [Test()]
        public void ChangeState_sameLocation()
        {
            var motionService = new MotionService(null, null, (change) => Console.WriteLine(change), null);
            var usr = "/api/v1/user/2/";

            motionService.ChangeState(new Rules.Library.Domain.MotionEvent() { Owner = usr, Timestamp = DateTime.UtcNow, Location = "BEDROOM" });
            motionService.ChangeState(new Rules.Library.Domain.MotionEvent() { Owner = usr, Timestamp = DateTime.UtcNow, Location = "BEDROOM" });
        }

        [Test()]
        public void SheduleSleepCheckForBedroom_RuleEngine()
        {

            var activityLog = new ActivityLog();
            var usr = "/api/v1/user/2/";

            var inform = new MockInform(null, null, activityLog);

            var ruleHandler = new RuleHandler(inform, activityLog);
            activityLog.ActivityRuleHandler = ruleHandler.ActivityHandler;

            ruleHandler.HandleLocationChange(new LocationChange(usr, "KITCHEN", "BEDROOM"));
        }

    }
}

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


            var ruleHandler = new RuleHandler();

            var inform = new Inform(new MockStoreAPI(), null);
            var service = new BathroomVisitService(inform, ruleHandler.BathroomVisitsDayHandler);


            service.Get(2);
        }
    }
}

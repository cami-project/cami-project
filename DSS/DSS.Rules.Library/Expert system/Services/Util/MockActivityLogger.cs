using System;
namespace DSS.Rules.Library
{
    public class MockActivityLogger
    {
        
        public bool EventOfTypeHappened(string owner, ActivityType activityType, int min)
        {
            Console.WriteLine("INVOKED!!!!");

            return true;
        }
    }
}

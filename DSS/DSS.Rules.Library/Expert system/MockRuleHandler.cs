using System;
namespace DSS.Rules.Library
{
    public class MockRuleHandler : IHandler
    {
        
        public void Handle(object e)
        {
            Console.WriteLine("[Mock rule handler]: " + e);
        }
    }
}

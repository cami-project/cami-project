using System;
using DSS.Delegate;

namespace DSS.FuzzyInference
{
    public class FuzzyHandler : IRouterHandler<Event>
    {
        public string Name => "FUZZY-INFERENCE";

        public void Handle(Event obj)
        {

            Console.WriteLine("OVO je fuzzy inference");

        }
    }
}

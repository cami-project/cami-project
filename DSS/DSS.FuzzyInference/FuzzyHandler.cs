using System;
using DSS.Delegate;


namespace DSS.FuzzyInference
{
    public class FuzzyHandler : IRouterHandler<Event>
    {
        public TimerQueue<Event> Queue { get; set; }

	    public string Name => "FUZZY-INFERENCE";

        public void Handle(Event obj)
        {

            Queue.Refresh();

            Console.WriteLine("OVO je fuzzy inference");


            Queue.Push(obj, 1);

        }
    }
}

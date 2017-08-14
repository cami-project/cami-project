using System;
using DSS.Delegate;

namespace DSS.FuzzyInference.Playground
{
    class MainClass
    {
        public static void Main(string[] args)
        {

            var timerQueue = new TimerQueue<Event>();


   //         timerQueue.Push(new Event("event 1","type 1", "1", "device"), -1 );
			//timerQueue.Push(new Event("event 2", "type 1", "1", "device"), 1);
			//timerQueue.Push(new Event("event 3", "type 1", "1", "device"), -2);
			//timerQueue.Push(new Event("event 4", "type 1", "1", "device"), -5);
			//timerQueue.Push(new Event("event 5", "type 1", "1", "device"), 1);


            //every time you recieve a new event first refreshh
            timerQueue.Refresh();

            foreach (var item in timerQueue.Queue)
            {
                Console.WriteLine(item.ToString());
			}

        }
    }
}

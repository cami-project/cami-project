using System;
using System.Collections.Generic;
using DSS.Delegate;
using System.Linq;
using DSS.RMQ;
using Newtonsoft.Json;

namespace DSS.FuzzyInference
{
    public class FallHandler : IRouterHandler
    {
        public TimerQueue<Event> Queue { get; set; }
	    public string Name => "FALL";
        private FuzzyContainer Fuzzy;

        public FallHandler()
        {
            Queue = new TimerQueue<Event>();
            Fuzzy = new FuzzyContainer();
        }

        public void Handle(string json)
        {
			Console.WriteLine("Fuzzy inference invoked (Fall detection)...");
			Console.WriteLine("Object handled by fall: " + json );

            var obj = JsonConvert.DeserializeObject<Event>(json);

            Queue.Refresh();
            Queue.Push(obj, 15);

            var inferenceResult = Fuzzy.Infer(Queue.ToNormal());

            if (inferenceResult.Count == 0)
				Console.WriteLine("NO RESULTS");
            else 
			    Console.WriteLine("Final decision: " + inferenceResult.Count);
            foreach (var item in inferenceResult)
            {
				Console.WriteLine("---------------");
				Console.WriteLine(item);
			}

        }
    }





}

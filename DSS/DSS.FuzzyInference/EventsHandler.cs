using System;
using System.Collections.Generic;
using DSS.Delegate;
using Newtonsoft.Json;

namespace DSS.FuzzyInference
{
    public class EventsHandler : IRouterHandler
    {
		public TimerQueue<Event> Queue { get; set; }
		public List<Event> RequestManagableQueue { get; set; }

		private List<string> WhitelistManagebleQueueItems { get; set; }
        public string Name => "EVENT";

        public EventsHandler()
        {
            Queue = new TimerQueue<Event>();
			RequestManagableQueue = new List<Event>();
        }

        public void Handle(string json)
        {
			Console.WriteLine("EVENT handler invoked...");

			var obj = JsonConvert.DeserializeObject<Event>(json);
            var result = "";

			Queue.Refresh();

			if (obj.category == "USER_INPUT")
			{
                if(obj.content.name == "EXERCISE_MODE_ON")
				    RequestManagableQueue.Add(obj);

                if (obj.content.name == "EXERCISE_MODE_OFF")
                    RequestManagableQueue.RemoveAll(x => x.content.name == "EXERCISE_MODE_ON");
                
            }
            else  
            {
                Queue.Push(obj, 1);
            }

            if (obj.category == "HEART_RATE")
			{
                result = "Abnormal heart rate";

					foreach (var item in RequestManagableQueue)
					{
						if (item.content.name == "EXERCISE_MODE_ON")
						{
                            result = "";
						}
					}
			}

            Console.WriteLine("EVENT HANDLER RESULT: " + result);
        }
    }
}

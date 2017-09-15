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
        private RMQ.INS.InsertionAPI api; 

        public EventsHandler()
        {
            Queue = new TimerQueue<Event>();
			RequestManagableQueue = new List<Event>();

            api = new RMQ.INS.InsertionAPI("http://cami-insertion:8010/api/v1/insertion"); 
        
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
                {
					RequestManagableQueue.RemoveAll(x => x.content.name == "EXERCISE_MODE_ON");
                    Queue.Push(new Event(){ category = "SYSTEM", content = new Content() { name = "POST_EXERCISE"}}, 10);
                }
            }
            else  
            {
                Queue.Push(obj, 1);
            }

            if (obj.category == "HEART_RATE")
			{
                result = "ABNORMAL_HEART_RATE";

				foreach (var item in RequestManagableQueue)
				{
					if (item.content.name == "EXERCISE_MODE_ON")
					{
                        result = "";
					}
				}

                foreach (var item in Queue.Queue)
                {
                    if(item.Value.content.name == "POST_EXERCISE") 
                    {
                        result = "";
                    }
                }
            }

            if(result == "ABNORMAL_HEART_RATE") {
                
                api.InsertPushNotification( JsonConvert.SerializeObject( new DSS.RMQ.INS.PushNotification() { message = "Abnormal heart rate", user_id = 2}  ) );

				Console.WriteLine("EVENT HANDLER RESULT: " + result);
			}

        }
    }
}

using System;
using System.Collections.Generic;
using DSS.Delegate;
using System.Linq;
using DSS.RMQ;

namespace DSS.FuzzyInference
{
    public class FuzzyHandler : IRouterHandler<Event>
    {
        public TimerQueue<Event> Queue { get; set; }
        public List<Event> RequestManagableQueue { get; set; }

	    public string Name => "FUZZY-INFERENCE";


        public FuzzyHandler()
        {
            Queue = new TimerQueue<Event>();
            RequestManagableQueue = new List<Event>();
        }

        public void Handle(Event obj)
        {
            Console.WriteLine("OVO je fuzzy inference");

            Queue.Refresh();

            if (obj.category == "USER-INPUT")
            {

                //add or remove (later)
                RequestManagableQueue.Add(obj);
            }
            else
            {
                Queue.Push(obj, 1);
            }

            // post-processing 

            var inferenceResult = "Heartrate-High";


            if (inferenceResult == "Heartrate-High")
            {


                foreach (var item in RequestManagableQueue)
                {
                    if( item.content.name == "EXERCISE_MODE")
                    {
						inferenceResult = "";
					}
                }

            }


            var fakeJSON = "{  user_id: 2,  message: \"Your blood pressure is way too low!\"}";


            var api = new RmqAPI("http://141.85.241.224:8010/api/v1/insertion");

            api.PushNotification(fakeJSON);

            
        }
    }





}

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

        private List<string> WhitelistManagebleQueueItems { get; set; }
        private FuzzyContainer Fuzzy;


        public FuzzyHandler()
        {
            Queue = new TimerQueue<Event>();
            RequestManagableQueue = new List<Event>();

            WhitelistManagebleQueueItems = new List<string>();
            WhitelistManagebleQueueItems.Add("EXERCISE_MODE_OFF");
            Fuzzy = new FuzzyContainer();
        }

        public void Handle(Event obj)
        {
            Console.WriteLine("Fuzzy inference invoked...");

            Queue.Refresh();

            if (obj.category == "USER-INPUT" && !WhitelistManagebleQueueItems.Contains(obj.content.name))
            {
               RequestManagableQueue.Add(obj);
            }
            else
            {
                Queue.Push(obj, 1);
            }

            var inferenceResult = Fuzzy.Infer(Queue.ToNormal());


            for (int i = 0; i < inferenceResult.Count; i++)
            {

                if (inferenceResult[i] == "HEART_RATE-High" || inferenceResult[i] == "HEART_RATE-Low" || inferenceResult[i] == "HEART_RATE-Medium")
				{

                    if (inferenceResult[i] == "HEART_RATE-Medium")
						inferenceResult[i] = "";


					foreach (var item in RequestManagableQueue)
					{
						if (item.content.name == "EXERCISE_MODE_ON")
						{
							inferenceResult[i] = "";
						}
					}

					if (Queue.Queue.FirstOrDefault(x => x.Value.content.name == "EXERCISE_MODE_OFF") != null)
					{
						inferenceResult[i] = "";
					}


                    if (inferenceResult[i] != ""){

                        if (!new RmqAPI("").AreLastNHeartRateCritical(3, 50, 80))
                            inferenceResult[i] = "";
                    }
				}
            }

            inferenceResult.RemoveAll( x=> x == "");


            Console.BackgroundColor = ConsoleColor.Red;

            if (inferenceResult.Count == 0)
				Console.WriteLine("NO RESULTS");
            else 
			    Console.WriteLine("Final decision: " + inferenceResult.Count);
            foreach (var item in inferenceResult)
            {
				Console.WriteLine("---------------");

				Console.WriteLine(item);

			}

            Console.BackgroundColor = ConsoleColor.White;

            //var fakeJSON = "{  user_id: 2,  message: \"Your blood pressure is way too low!\"}";
            //var api = new RmqAPI("http://141.85.241.224:8010/api/v1/insertion");

            //api.PushNotification(fakeJSON);
        }
    }





}

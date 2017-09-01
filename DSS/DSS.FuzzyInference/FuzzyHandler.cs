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
        private List<string> IgnoredByFuzzy { get; set; }
        private FuzzyContainer Fuzzy;


        public FuzzyHandler()
        {
            Queue = new TimerQueue<Event>();
            RequestManagableQueue = new List<Event>();
            IgnoredByFuzzy = new List<string>();

            WhitelistManagebleQueueItems = new List<string>();
            WhitelistManagebleQueueItems.Add("EXERCISE_MODE_OFF");

            IgnoredByFuzzy.Add("MEASUREMENT");

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
                if(!IgnoredByFuzzy.Contains(obj.category))
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

			if (obj.category == "MEASUREMENT")
			{

				if (obj.content.name == "weight")
				{
					var kg = new RmqAPI("").GetLatestWeightMeasurement();

                    if (Math.Abs(obj.content.val.numVal - kg) > 2){
                        
                        var msg = obj.content.val.numVal > kg ? "Have lighter meals" : "Have more consistent meals";
                        inferenceResult.Add("Abnormal change in weight noticed - " +msg );
                        new RmqAPI("").PushJournalEntry(msg, "Abnormal change in weight noticed");
					}
				}
			}

            inferenceResult.RemoveAll( x=> x == "");

            if (inferenceResult.Count == 0)
				Console.WriteLine("NO RESULTS");
            else 
			    Console.WriteLine("Final decision: " + inferenceResult.Count);
            foreach (var item in inferenceResult)
            {
				Console.WriteLine("---------------");

				Console.WriteLine(item);

			}


            //var fakeJSON = "{  user_id: 2,  message: \"Your blood pressure is way too low!\"}";
            //var api = new RmqAPI("http://141.85.241.224:8010/api/v1/insertion");

            //api.PushNotification(fakeJSON);
        }
    }





}

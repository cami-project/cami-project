using System;
using DSS.Delegate;
using DSS.FuzzyInference;
using DSS.RMQ;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading;
using log4net;
using log4net.Config;
using System.Xml;
using System.Collections.Generic;

namespace DSS.Main
{
    
    class Program
    {


        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static void Main(string[] args)
        {
            
            Console.WriteLine(DateTime.Now.TimeOfDay);
            Console.WriteLine("DSS invoked...");


			IRouterHandler[] handlers =
            {
                new EventsHandler(),
				new MeasurementHandler(),
                new FallHandler(),
                new SensorToLocationHandler(),
                new ReminderHandler(),
                new MotionEventHandler()
			};


            // generate reminder event
            var reminderEvent = new Event()
            {
                category = "USER_NOTIFICATIONS",
                content = new Content()
                {
                    uuid = Guid.NewGuid().ToString(),
                    name = "exercise_started",
                    value_type = "complex",
                    val = new Dictionary<string, dynamic>()
                        {
                             { "user", new Dictionary<string, int>() { {"id", 2 } } },
                             { "exercise_type", "arm_gymnastics" },
                             { "session_uuid", "session 100" }
                        }
                },
                annotations = new Annotations()
                {
                    timestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                    source = "DSS"
                }
            };

            var reminderEventEND = new Event()
            {
                category = "USER_NOTIFICATIONS",
                content = new Content()
                {
                    uuid = Guid.NewGuid().ToString(),
                    name = "exercise_ended",
                    value_type = "complex",
                    val = new Dictionary<string, dynamic>()
                        {
                             { "user", new Dictionary<string, int>() { {"id", 2 } } },
                             { "exercise_type", "arm_gymnastics" },
                             { "session_uuid", "session 100" }
                        }
                },
                annotations = new Annotations()
                {
                    timestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                    source = "DSS"
                }
            };




            handlers[4].Handle(JsonConvert.SerializeObject(reminderEvent));
            handlers[4].Handle(JsonConvert.SerializeObject(reminderEventEND));



            return;


            var url = "amqp://cami:cami@cami-rabbitmq:5672/cami";
            try
            {
                var rmqEvents = new RmqExchange(url, "events", "event.*", (json) => { handlers[0].Handle(json); handlers[4].Handle(json); handlers[5].Handle(json);  } );
                var rmqMeasurements = new RmqExchange(url, "measurements", "measurement.*", (json) => { handlers[1].Handle(json);  });
			}
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong with the rmq exchange: " +  ex);
            }


			while (true)
            {

            }

        }

    }
}

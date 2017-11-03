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

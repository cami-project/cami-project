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
using DSS.RMQ;

namespace DSS.Main
{
    
    class Program
    {


		public static void Main(string[] args)
        {
            
            Console.WriteLine(DateTime.Now.TimeOfDay);
            Console.WriteLine("DSS invoked...");
            Console.WriteLine("This is version 1.0.8");

			var router = new Router<Event>();

			IRouterHandler[] handlers =
            {
				new FuzzyHandler(),
				new MeasurementHandler()
			};

            var url = "amqp://cami:cami@cami-rabbitmq:5672/cami";
            try
            {
                var rmqEvents = new RmqExchange(url, "events", "event.*", (json)=> { handlers[0].Handle(json); });
				var rmqMeasurements = new RmqExchange(url, "measurements", "measurement.*", (json) => { handlers[1].Handle(json); });

			}
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong iwth the rmq exchange: " +  ex);
            }

			//Testing measuremnts 


			var measure = new Measurement()
			{
			    device = "/api/v1/device/2/",
			    id = "200",
			    measurement_type = "pulse",
			    resource_uri = "/api/v1/measurement/1/",
			    timestamp = 1477413397,
			    unit_type = "bpm",
			    user = "/api/v1/user/2/",
			    value_info = "200"
			};

            handlers[1].Handle(JsonConvert.SerializeObject(measure) );



			while (true)
            {

            }

        }

    }
}

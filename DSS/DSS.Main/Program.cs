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

namespace DSS.Main
{
    
    class Program
    {


		public static void Main(string[] args)
        {
            
            Console.WriteLine(DateTime.Now.TimeOfDay);
            Console.WriteLine("DSS invoked...");
            Console.WriteLine("This is version 1.0.5");

			var router = new Router<Event>();

   //         var url = "amqp://cami:cami@cami-rabbitmq:5672/cami";
   //         try
   //         {
			//	var rmqExchange = new RmqExchange(url, null);
			//}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Something went wrong iwth the rmq exchange: " +  ex);
            //}


            IRouterHandler[] handlers =
            {
                new ConsolePrintHandler<Event>(),
                new FuzzyHandler(),
                new MeasurementHandler()
			};

            //var config = new Config("default.config", router, handlers);

            //handlers[1].Handle(new Event("Heart-Rate", new Content() { name = "Heart-Rate", val = new Value() { numVal = 60 } }, new Annotations()));
            // handlers[1].Handle(new Event("MEASUREMENT", new Content() { name = "weight", val = new Value() { numVal = 69 } }, new Annotations()));


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

            handlers[2].Handle(measure);


			while (true)
            {

            }

        }

    }
}

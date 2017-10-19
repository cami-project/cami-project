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
using System.Reflection;
using System.IO;

namespace DSS.Main
{
    
    class Program
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(DSS.Main.Program));

		public static void Main(string[] args)
        {

            string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string xslLocation = Path.Combine(executableLocation, "log4net.config.xml");


            XmlConfigurator.Configure(new System.IO.FileInfo(xslLocation));

            log.Info("Entering application.");
            log.Info("Exiting application.");


            Console.WriteLine(DateTime.Now.TimeOfDay);
            Console.WriteLine("DSS invoked...");
            Console.WriteLine("This is version 1.2");


			var router = new Router<Event>();
			IRouterHandler[] handlers =
            {
                new EventsHandler(),
				new MeasurementHandler(),
                new FallHandler(),
                new SensorToLocationHandler()
			};

            var url = "amqp://cami:cami@cami-rabbitmq:5672/cami";
            try
            {
                var rmqEvents = new RmqExchange(url, "events", "event.*", (json) => { handlers[0].Handle(json); handlers[2].Handle(json); handlers[3].Handle(json); } );
                var rmqMeasurements = new RmqExchange(url, "measurements", "measurement.*", (json) => { handlers[1].Handle(json);  });

			}
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong with the rmq exchange: " +  ex);
            }



            //var store = new StoreAPI("http://141.85.241.224:8008/api/v1");

            //store.AreLastNHeartRateCritical(3, 50, 180);


			while (true)
            {

            }

        }

    }
}

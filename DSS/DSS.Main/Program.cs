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


            var insertionAPI = new RMQ.INS.InsertionAPI("http://cami-insertion:8010/api/v1/insertion");

            var notificationMsg = @"{
              ""user_id"": 2,
              ""message"": ""Your blood pressure is way too low!""
            }";
            
            insertionAPI.InsertPushNotification(notificationMsg);


			while (true)
            {

            }

        }

    }
}

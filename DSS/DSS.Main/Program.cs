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
using System.Linq;

namespace DSS.Main
{
    
    class Program
    {
        
		public static void Main(string[] args)
        {
            
            Console.WriteLine(DateTime.Now.TimeOfDay);
            Console.WriteLine("DSS invoked...##");

            var url = "amqp://cami:cami@cami-rabbitmq:5672/cami";
            try
            {
                var rmqEvents = new RmqExchange(url, "events", "event.*", (json) => { 
                    new ReminderHandler().Handle(json); 
                    new MotionEventHandler().Handle(json); 
                    new FallDetectionHandler().Handle(json); 
                } );
                var rmqMeasurements = new RmqExchange(url, "measurements", "measurement.*", (json) => { new MeasurementHandler().Handle(json);  });
			}
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong with the rmq exchange: " +  ex);
            }

            Console.WriteLine("RMQ binding done!");


			while (true)
            {

            }

        }

    }
}

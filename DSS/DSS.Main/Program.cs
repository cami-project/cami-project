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


		public static void Main(string[] args)
        {
            
            Console.WriteLine(DateTime.Now.TimeOfDay);
            Console.WriteLine("DSS invoked...");
            Console.WriteLine("This is version 1.2");



            new MeasurementHandler();


            Console.ReadLine();
            return;

            var store = new StoreAPI("http://cami-store:8008");

            var je = store.PushJournalEntry("/api/v1/user/2/", "blood_pressure", "low", "blood pressure", "blood_pressure");

        
            var reminderEvent = new Event()
            {
                category = "USER_NOTIFICATIONS",
                content = new Content()
                {
                    uuid = Guid.NewGuid().ToString(),
                    name = "reminder_sent",
                    value_type = "complex",
                    val = new Dictionary<string, dynamic>()
                        {
                             { "user", new Dictionary<string, int>() { {"id", 2 } } },
                             { "journal", new Dictionary<string, dynamic>() {
                                 { "id_enduser", 2 },
                                 { "id_caregivers", new [] {2}}
                             } },
                        }
                },
                annotations = new Annotations()
                {
                    timestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                    source = "DSS"
                }
            };


            var reminderACK = new Event()
            {
                category = "USER_NOTIFICATIONS",
                content = new Content()
                {
                    uuid = Guid.NewGuid().ToString(),
                    name = "reminder_acknowledged",
                    value_type = "complex",
                    val = new Dictionary<string, dynamic>()
                    {   { "ack" , "ok"},
                        { "user", new Dictionary<string, int>() { {"id", 2 } } },
                        { "journal", new Dictionary<string, dynamic>() {{ "id", je.id }} },
                    }
                },
                annotations = new Annotations()
                {
                    timestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                    source = "ios_app"
                }
            };




            var mesuremtent = new Measurement()
            {
                unit_type = "bpm",
                device = "/api/v1/device/2/",
                ok = true,
                user = "/api/v1/user/2/",
                value_info = new BloodPressureValueInfo()
                {
                    diastolic = 10,
                    pulse = 100,
                    systolic = 5
                },
                measurement_type = "blood_pressure",
                gateway_id = null,
                timestamp = (Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds

            };

       

            var handler = new ReminderHandler();
            handler.Handle(JsonConvert.SerializeObject(reminderEvent));
            handler.Handle(JsonConvert.SerializeObject(reminderACK));


            store.PushMeasurement(JsonConvert.SerializeObject(mesuremtent));


            Console.ReadLine();





			var router = new Router<Event>();
			IRouterHandler[] handlers =
            {
                new EventsHandler(),
				new MeasurementHandler(),
                new FallHandler(),
                new SensorToLocationHandler(),
                new ReminderHandler()
			};

            var url = "amqp://cami:cami@cami-rabbitmq:5672/cami";
            try
            {
                //var rmqEvents = new RmqExchange(url, "events", "event.*", (json) => { handlers[0].Handle(json); handlers[2].Handle(json); handlers[3].Handle(json); } );
                var rmqEvents = new RmqExchange(url, "events", "event.*", (json) => { handlers[0].Handle(json); handlers[4].Handle(json);  } );
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

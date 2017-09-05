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
            Console.WriteLine("This is version 1.0.6");

			var router = new Router<Event>();

            var url = "amqp://cami:cami@cami-rabbitmq:5672/cami";
            try
            {
				var rmqExchange = new RmqExchange(url, null);
			}
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong iwth the rmq exchange: " +  ex);
            }


            IRouterHandler[] handlers =
            {
                new ConsolePrintHandler<Event>(),
                new FuzzyHandler(),
                new MeasurementHandler()
			};

            //var config = new Config("default.config", router, handlers);

            //handlers[1].Handle(new Event("Heart-Rate", new Content() { name = "Heart-Rate", val = new Value() { numVal = 60 } }, new Annotations()));
            // handlers[1].Handle(new Event("MEASUREMENT", new Content() { name = "weight", val = new Value() { numVal = 69 } }, new Annotations()));


            //var measure = new Measurement()
            //{
            //    device = "/api/v1/device/2/",
            //    id = "200",
            //    measurement_type = "pulse",
            //    resource_uri = "/api/v1/measurement/1/",
            //    timestamp = 1477413397,
            //    unit_type = "bpm",
            //    user = "/api/v1/user/2/",
            //    value_info = "200"
            //};

            //handlers[2].Handle(measure);


            var insertionAPI = new RMQ.INS.InsertionAPI("http://cami-insertion:8010/api/v1/insertion");





            var anEvent = JsonConvert.DeserializeObject<RMQ.INS.Event>(@"{
  ""category"": ""USER_ENVIRONMENT"",
  ""content"": {
    ""name"": ""presence_detected | kitchen_window"",
    ""value_type"": ""integer"",
    ""value"": {
      ""user"": {
        ""name"": ""Jim"",
        ""uri"": ""/v1/resources/user/1/""
      },
      ""room"": {
        ""name"": ""Kitchen""
      }
    }
  },
  ""annotations"": {
    ""timestamp"": 0,
    ""source"": [
      ""DSS | /v1/resources/devices/3/""
    ],
    ""certainty"": 0,
    ""temporal_validity"": {
      ""start_ts"": 0,
      ""end_ts"": 0
    }
  }
}");

            //Console.Write(anEvent.content.VALUE.user.name = "EZEL");
            insertionAPI.InsertEvent(JsonConvert.SerializeObject(anEvent));

			while (true)
            {

            }

        }

    }
}

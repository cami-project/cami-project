using System;
using DSS.Delegate;
using DSS.FuzzyInference;
using DSS.RMQ;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace DSS.Main
{



    class Program
    {
        
        public static void PushToAPI()
        {
            var client = new HttpClient();
            HttpContent content = new StringContent(new Event("Fall").ToJson());   //new StringContent("{ \"measurement_type\": \"AAAAAAAAAAAAAAAAAAAAAAA\", \"unit_type\": \"kg\",\"timestamp\": 0,\"user\": \"/api/v1/user/14/\",\"device\": \"/api/v1/device/1/\",\"value_info\": \"{'systolic': 115, 'diastolic': 70}\"}", Encoding.UTF8, "application/json");


			//139.59.181.210
			//141.85.241.224

			//in measruments chanel works
			//var response = client.PostAsync("http://141.85.241.224:8010/api/v1/insertion/measurements/", content);


			var response = client.PostAsync("http://141.85.241.224:8010/api/v1/insertion/events/", content);


            Console.WriteLine(response.Result);

        }

        private static void ReadFromExchange()
        {


        }

        public static void Main(string[] args)
        {
            Console.WriteLine("DSS invoked...");
			Console.WriteLine("Connecting to the msg broker...");

            PushToAPI();


			var router = new Router<Event>();

            var rmq = new Rmq<Event>("amqp://cami:cami@141.85.241.224:5673/cami",
                              "cami",
                              "cami",
                              "test-queue",
                              (result) =>
                              {
                                Console.WriteLine("RECIEVED: " + result);
                                router.Handle(JsonConvert.DeserializeObject<Event>(result));
                             } );

			var rmqExchange = new RmqExchange("amqp://cami:cami@141.85.241.224:5673/cami",
				  "cami",
				  "cami",
				  "measurements",
				  (result) =>
				  {
					  Console.WriteLine("RECIEVED EXCHANGE: " + result);
					  //router.Handle(JsonConvert.DeserializeObject<Event>(result));
				  });





            IRouterHandler<Event>[] handlers =
            {
                new ConsolePrintHandler<Event>(),
                new PushNotificationHandler<Event>(new RmqWriter<Event>("amqp://cami:cami@141.85.241.224:5673/cami", "cami", "cami", "frontend_notifications")),
                new FuzzyHandler()
			};

            var config = new Config("default.config", router, handlers);


            var run = true;

            while(run)
            {
				Console.WriteLine("=========================");
                Console.WriteLine("[1] - Generate default fall event {\"Category\":\"Fall\"}");
				Console.WriteLine("[2] - Generate fall event with custom data");

				Console.WriteLine("ctrl + c - Exit");
				Console.WriteLine("=========================");

                var num = Console.ReadLine();

                if (num == "1")
					// rmq.Write(JsonConvert.SerializeObject(new Event("Fall")));
                    rmq.Write(new Event("Fall").ToJson());


				if (num == "2")
                {
                    var input = new string [4];
                    Console.Write("Category: ");
                    input[0] = Console.ReadLine();

					//Console.Write("Type: ");
					//input[1] = Console.ReadLine();

					//Console.Write("Value: ");
					//input[2] = Console.ReadLine();

					//Console.Write("Device: ");
					//input[3] = Console.ReadLine();

					//rmq.Write(new Event(input[0], input[1], input[2], input[3]).ToJson());


                    rmq.Write(new Event(input[0]).ToJson());
				}

                run = num != "3";
			}
            rmq.Dispose();

        }

    }
}

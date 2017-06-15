using System;
using DSS.Delegate;
using DSS.FuzzyInference;
using DSS.RMQ;
using Newtonsoft.Json;

namespace DSS.Main
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("DSS invoked...");
			Console.WriteLine("Connecting to the msg broker...");

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
                Console.WriteLine("[1] - Generate default fall event {\"Name\":\"Fall\",\"Type\":\"Fall\",\"Val\":\"High\"}");
				Console.WriteLine("[2] - Generate fall event with custom data");

				Console.WriteLine("ctrl + c - Exit");
				Console.WriteLine("=========================");

                var num = Console.ReadLine();

                if (num == "1")
                    rmq.Write(new Event("Fall", "Fall", "High", "Device").ToJson());

                if (num == "2")
                {
                    var input = new string [4];
                    Console.Write("Name: ");
                    input[0] = Console.ReadLine();

					Console.Write("Type: ");
					input[1] = Console.ReadLine();

					Console.Write("Value: ");
					input[2] = Console.ReadLine();

					Console.Write("Device: ");
					input[3] = Console.ReadLine();

                    rmq.Write(new Event(input[0], input[1], input[2], input[3]).ToJson());

				}

                run = num != "3";
			}
            rmq.Dispose();

        }

    }
}

using System;
using DSS.Delegate;
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
                new PushNotificationHandler<Event>(new RmqWriter<Event>("amqp://cami:cami@141.85.241.224:5673/cami", "cami", "cami", "frontend_notifications"))
			};

            var config = new Config("default.config", router, handlers);


            var run = true;

            while(run)
            {
				Console.WriteLine("=========================");
                Console.WriteLine("[1] - Generate default fall event {\"Name\":\"Fall\",\"Type\":\"Fall\",\"Val\":\"High\"}");
                Console.WriteLine("ctrl + c - Exit");
				Console.WriteLine("=========================");

                var num = Console.ReadLine();

                if (num == "1")
                    rmq.Write(new Event("Fall", "Fall", "High").ToJson());

                if (num == "2")
                    return;

                run = num != "2";
			}
            rmq.Dispose();

        }

    }
}

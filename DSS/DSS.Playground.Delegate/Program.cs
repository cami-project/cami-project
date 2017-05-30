using System;
using DSS.Delegate;
using System.Linq;
using DSS.RMQ;

namespace DSS.Playground
{
    class Program
    {
        public static void Main(string[] args)
        {


            var router = new Router<Event>();

            IRouterHandler<Event> [] handlers = 
            { 
                new ConsolePrintHandler<Event>(), 
                new PushNotificationHandler<Event>(new RmqWriter<Event>("", "", "", "")) 
            };
			
            var config = new Config("default.config", router, handlers);

            router.Handle(new Event("Fall", "Fall", "5"));

        }
    }
}

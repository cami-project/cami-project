using System;
using DSS.Delegate;

namespace DSS.Playground
{
    class MainClass
    {
        public static void Main(string[] args)
        {

            var router = new Router<Event>();

            router.RegisterChannel("Console", new ConsolePrintHandler<Event>());

            var e = new Event("Fall", "Fall", "High");

            router.RegisterEvent("Console", e);

            router.Handle(e);




        }
    }
}

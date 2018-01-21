using System;
using DSS.Delegate;
using DSS.FuzzyInference;
using DSS.RMQ;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Timers;
using log4net;
using log4net.Config;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using DSS.Rules.Library;

namespace DSS.Main
{
    
    class Program
    {
        
		public static void Main(string[] args)
        {
            

            Console.WriteLine(DateTime.Now.TimeOfDay);
            Console.WriteLine("DSS invoked...##");


            var ruleHandler = new RuleHandler();
            SheduleService.OnExec = ruleHandler.HandleSheduled;

            var fallEv = new DSS.Rules.Library.Event(){ content = new Rules.Library.Content(){ name = "fall"}, annotations = new Rules.Library.Annotations(){ source = new {
                        gateway = "/api/v1/gateway/1/"
                    }}};

            ruleHandler.HandleEvent(JsonConvert.SerializeObject(fallEv));

            var url = "amqp://cami:cami@cami-rabbitmq:5672/cami";
            try
            {
                var rmqEvents = new RmqExchange(url, "events", "event.*", (json) => { ruleHandler.HandleEvent(json); });
                var rmqMeasurements = new RmqExchange(url, "measurements", "measurement.*", (json) => { ruleHandler.HandleMeasurement(json); });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong with the rmq exchange: " + ex);
            }


            //Invoke every 30 sec to check if there is any sheduled event 
            Timer timer = new Timer
            {
                Interval = 30 * 1000
            };
            timer.Elapsed += (x, y) => { SheduleService.Update(); };
            timer.AutoReset = true;
            timer.Start();

            //Make initial sheduling 
            var newDayEven = new SheduledEvent() { type = SheduleService.Type.NewDay };
            ruleHandler.HandleSheduled(newDayEven);

            // Invoke every day
            Timer timerToInvokeEveryDayTimer = new Timer();
            timerToInvokeEveryDayTimer.Interval = (DateTime.UtcNow.Date.AddDays(1) - DateTime.UtcNow).TotalMilliseconds;
            timerToInvokeEveryDayTimer.AutoReset = false;
            timerToInvokeEveryDayTimer.Elapsed += (x, y) => {

                Timer everyDayTimer = new Timer();
                everyDayTimer.Interval = 86400000;
                everyDayTimer.AutoReset = true;
                everyDayTimer.Elapsed += (z, c) =>
                {

                    Console.WriteLine("A new day even raised: " + DateTime.UtcNow);
                    ruleHandler.HandleSheduled(new SheduledEvent() { type = SheduleService.Type.NewDay });
                };
                everyDayTimer.Start();
            };
            timerToInvokeEveryDayTimer.Start();


            Console.ReadKey();





   //         var reminderHandler = new ReminderHandler();
   //         var motionHandler = new MotionEventHandler();
   //         var fallEventHandler = new FallDetectionHandler();
   //         var measurementHandler = new MeasurementHandler();

   //         var url = "amqp://cami:cami@cami-rabbitmq:5672/cami";
   //         try
   //         {
   //             var rmqEvents = new RmqExchange(url, "events", "event.*", (json) => { 

   //                 reminderHandler.Handle(json); 
   //                 motionHandler.Handle(json);
   //                 fallEventHandler.Handle(json); 
   //             } );
   //             var rmqMeasurements = new RmqExchange(url, "measurements", "measurement.*", (json) => { measurementHandler.Handle(json);  });
			//}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Something went wrong with the rmq exchange: " +  ex);
            //}

            //Console.WriteLine("RMQ binding done!");


			while (true)
            {

            }

            Console.WriteLine("Exiting DSS...");

        }

    }
}

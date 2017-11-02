using System;
using System.Timers;
using DSS.Delegate;

namespace DSS.FuzzyInference.Playground
{
    class MainClass
    {
        public static void Main(string[] args)
        {

            //var timerQueue = new TimerQueue<Event>();


            //         timerQueue.Push(new Event("event 1","type 1", "1", "device"), -1 );
            //timerQueue.Push(new Event("event 2", "type 1", "1", "device"), 1);
            //timerQueue.Push(new Event("event 3", "type 1", "1", "device"), -2);
            //timerQueue.Push(new Event("event 4", "type 1", "1", "device"), -5);
            //timerQueue.Push(new Event("event 5", "type 1", "1", "device"), 1);


            //every time you recieve a new event first refreshh
            //timerQueue.Refresh();

            //foreach (var item in timerQueue.Queue)
            //{
            //    Console.WriteLine(item.ToString());
            //}

            //MotionEventHandler handler = new MotionEventHandler();

            //string eventJsonStr = @"{""category"": ""USER_ENVIRONMENT"",
            //                         ""content"": 
            //                            {""name"": ""presence"",
            //                             ""value_type"": ""complex"",
            //                             ""value"": {""alarm_motion"":true, ""sensor_luminance"":64, ""sensor_temperature"":27.5, ""alarm_tamper"":false,""battery_level"":85}},
            //                         ""annotations"": {""timestamp"":1508396583, ""source"":{""gateway"":""/api/v1/gateway/1/"", ""sensor"": ""/api/v1/device/9/""}
            //                        }}";

            //handler.Handle(eventJsonStr);



            StartTimer(ChangeTime(DateTime.Now, 12, 4),new Timer());
            Console.ReadLine();

        }


        public static DateTime ChangeTime(DateTime date, int hour, int min){

            return date.Date + new TimeSpan(hour, min, 0);

        }

        public static void StartTimer(DateTime time, Timer timer)
        {


            var interval = (time - DateTime.Now).TotalMilliseconds;
            Console.WriteLine(interval);

            timer.Interval = interval;
            timer.Enabled = true;
            timer.AutoReset = false;
            timer.Elapsed += (sender, args) =>
            {
                Console.WriteLine("Invoked by the timer: " + DateTime.Now);
            };
        }



    }
}

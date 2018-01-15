using System;
using System.Collections.Generic;

namespace DSS.Rules.Library
{

    public class SheduledEvent
    {

        public string user;
        public SheduleService.Type type;
        public DateTime utcTime;
        public Action action;

        public bool isStepAnalisys()
        {

            return type == SheduleService.Type.Steps;
        }


        public bool Compare(DateTime time)
        {

            return utcTime.Hour == time.Hour && utcTime.Minute == time.Minute;
        }


        public bool isNewDay() {

            return type == SheduleService.Type.NewDay;
        }

    }


    public static class SheduleService
    {
        public enum Type
        {
            None,
            Steps,
            NewDay
        };

        private static List<SheduledEvent> sheduledEvents = new List<SheduledEvent>();


        public static Action<SheduledEvent> OnExec; 

        public static void OncePreDayAt(int hour, int min, Type type)
        {


        }


        public static void At(int hour, int min)
        {

            throw new NotImplementedException();
        }

        public static void Add(SheduledEvent e) {

            sheduledEvents.Add(e);
        }

 
        public static void In(int min, Action action, string usr)
        {

            Console.Write("IN Invoked");

            var e = new SheduledEvent()
            {
                action = action,
                utcTime = DateTime.UtcNow.AddMinutes(min),
                user = usr
            };

            sheduledEvents.Add(e);
        }

        public static void Remove(Type type, string user){

            sheduledEvents.RemoveAll(x=> x.type == type && x.user == user);

        }

        //This should be invoked every min 
        public static void Update()
        {

            Console.WriteLine("Update called! " + DateTime.UtcNow);

            var now = DateTime.UtcNow;
            var itemsToRemove = new List<SheduledEvent>();

            foreach (var e in sheduledEvents)
            {
                if (e.Compare(now))
                {

                    if(e.type== Type.None){
						e.action();
                    }else {

                        OnExec(e);
                    }

                    itemsToRemove.Add(e);
                }
            }

            itemsToRemove.ForEach( x => sheduledEvents.Remove(x));
        }


    }
}

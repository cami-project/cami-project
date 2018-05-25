using System;
using System.Collections.Generic;

namespace DSS.Rules.Library
{

    public class SheduledEvent : IEvent
    {
        public SheduleService.Type Type;
        public DateTime UtcTime;
        public Action Action;

        public string Owner { get; set; }
        public string Lang { get; set; }
        public string Timezone { get; set; }
        public DateTime Timestamp { get; set; }

        public SheduledEvent(IOwner e, SheduleService.Type type, DateTime utcTime )
        {
            this.Owner = e.Owner;
            this.Type = type;
            this.UtcTime = utcTime;
        }

        public SheduledEvent(string owner, SheduleService.Type type, DateTime utcTime)
        {
            this.Owner = owner;
            this.Type = type;
            this.UtcTime = utcTime;
            this.Action = null;
        }

        public SheduledEvent(string owner, Action action, DateTime utcTime) 
        {
            this.Owner = owner;
            this.Action = action;
            this.UtcTime = utcTime;
            this.Type = SheduleService.Type.Null;
        }

        public SheduledEvent (SheduleService.Type type) 
        {
            this.Type = type;
        }

        public bool Is(SheduleService.Type type)
        {
            return this.Type == type;
        }

        public bool isStepAnalisys()
        {
            return Type == SheduleService.Type.Steps;
        }
        public bool isSleepingCheck()
        {
            return Type == SheduleService.Type.SleepCheck;
        }

        public bool Compare(DateTime time)
        {
            return UtcTime.Hour == time.Hour && UtcTime.Minute == time.Minute;
        }

        public bool isNewDay() 
        {
            return Type == SheduleService.Type.NewDay;
        }

        public override string ToString()
        {
            return string.Format("Sheduled event for : {0} of type: {1} at {2}", this.Owner, this.Type, this.UtcTime);
        }
    }


    public static class SheduleService
    {
        public enum Type
        {
            Null,
            Steps,
            NewDay,
            CheckMovementAfterFall,
            SleepCheck,
            CheckForNightWandering,
            CheckIfLeftHouse,
            MorningWeightReminder,
            MorningBloodPressureReminder
        };

        public enum TimeOfDay 
        {
            None,
            Morning,
            Afternoon,
            Night
        }

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

 
        public static void In(int min, Action action, string owner)
        {

            Console.Write("IN Invoked");

            var e = new SheduledEvent(owner, action, DateTime.UtcNow.AddMinutes(min));
          

            sheduledEvents.Add(e);
        }

        public static void Remove(Type type, string user){

            sheduledEvents.RemoveAll(x=> x.Type == type && x.Owner == user);

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

                    Console.WriteLine("Match: " + e.Type);

                    if(e.Type== Type.Null){
						e.Action();
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

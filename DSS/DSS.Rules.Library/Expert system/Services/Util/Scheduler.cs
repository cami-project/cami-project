using System;
using System.Collections.Generic;

namespace DSS.Rules.Library
{
    public class Scheduler : IScheduler
    {
        //TODO: OnExec might be a bad design
        public Action<SheduledEvent> OnExec { get; set; }
        private IList<SheduledEvent> scheduled = new List<SheduledEvent>();

        public void Add(SheduledEvent e)
        {
            scheduled.Add(e);
        }

        public void IntervalInvoke()
        {
            Console.WriteLine("Interval invoked! " + DateTime.UtcNow);

            var now = DateTime.UtcNow;
            var itemsToRemove = new List<SheduledEvent>();

            foreach (var e in scheduled)
            {
                if (e.Compare(now))
                {

                    Console.WriteLine("Match: " + e.Type);

                    if (e.Type == SheduleService.Type.Null)
                    {
                        e.Action();
                    }
                    else
                    {
                        OnExec(e);
                    }

                    itemsToRemove.Add(e);
                }
            }

            itemsToRemove.ForEach(x => scheduled.Remove(x));
        }

    }
}

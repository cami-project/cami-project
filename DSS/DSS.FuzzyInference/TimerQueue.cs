using System;
using System.Collections.Generic;

namespace DSS.FuzzyInference
{
    public class TimerQueue<T>
    {
        public List<Item<T>> Queue { get; set; }

        public TimerQueue()
        {
            this.Queue = new List<Item<T>>();
        }

        public void Push(T item, float duration)
        {
            Queue.Add(new Item<T>(item, duration));
        }

        public void Refresh() 
        {
            this.Queue.RemoveAll(x => x.TimeStamp.AddMinutes(x.Duration) < DateTime.Now);
        }

        public override string ToString()
        {
            return string.Format("[TimerQueue: Queue={0}]", Queue.Count);
        }


        public class Item<T> 
        {
            public T Value { get; set; }
            public DateTime TimeStamp { get; set; }
            public float Duration { get; set; }


            public Item(T val)
            {
                this.Value = val;
                this.TimeStamp = DateTime.Now;
                this.Duration = 30;
            }

            public Item(T val, float duration)
			{
				this.Value = val;
				this.TimeStamp = DateTime.Now;
                this.Duration = duration;
			}

            public override string ToString()
            {
                return string.Format("[Item: Value={0}, TimeStamp={1}, Duration={2}]", Value, TimeStamp, Duration);
            }

        }
    }
}

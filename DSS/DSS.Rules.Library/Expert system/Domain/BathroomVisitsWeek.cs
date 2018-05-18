using System;
using System.Collections.Generic;
using System.Linq;

namespace DSS.Rules.Library
{
    public class BathroomVisitsWeek
    {
        readonly public int Yesteday;
        readonly List<int> Days;

        public BathroomVisitsWeek(int yesteday, List<int> daysOfWeek)
        {
            this.Yesteday = yesteday;
            this.Days = daysOfWeek;
        }

        public bool isValid()
        {
            return !this.Days.Contains(-1);
        }

        public bool tresholdFromAverage(int treshold)
        {
            return Math.Abs(this.Days.Average() - this.Yesteday) > treshold;
        }

        public override string ToString()
        {
            return string.Format("Is valid: {0}, week days added {1} ,avg value: {2}, yesterday: {3}", isValid(), Days.Count, Days.Average(), Yesteday);
        }

    }
}

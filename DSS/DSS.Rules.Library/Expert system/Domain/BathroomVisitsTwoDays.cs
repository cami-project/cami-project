using System;
namespace DSS.Rules.Library
{
    public class BathroomVisitsTwoDays
    {
        readonly public int ? Yesterday;
        readonly public int ? DayBeforeYesterday;

        public BathroomVisitsTwoDays(int yesterday, int dayBeforeYesterday)
        {
            this.Yesterday = yesterday;
            this.DayBeforeYesterday = dayBeforeYesterday;
        }

        public bool isValid()
        {
            return Yesterday.HasValue && DayBeforeYesterday.HasValue;
        }

        public bool isOverTrehshold(int treshold)
        {
            return Math.Abs(Yesterday.Value - DayBeforeYesterday.Value) > treshold;
        }

        public override string ToString()
        {
            return string.Format("Yesterday: {0} & Day before yesteday: {1}", Yesterday, DayBeforeYesterday);
        }

    }
}

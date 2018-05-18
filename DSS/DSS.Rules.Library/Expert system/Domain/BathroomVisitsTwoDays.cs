using System;
namespace DSS.Rules.Library
{
    public class BathroomVisitsTwoDays
    {
        readonly public int Yesterday;
        readonly public int DayBeforeYesterday;

        public BathroomVisitsTwoDays(int yesterday, int dayBeforeYesterday)
        {
            this.Yesterday = yesterday;
            this.DayBeforeYesterday = dayBeforeYesterday;
        }

        public bool isValid()
        {
            return Yesterday > 0 && DayBeforeYesterday > 0;
        }

        public bool isOverTrehshold(int treshold)
        {
            return Math.Abs(Yesterday - DayBeforeYesterday) > treshold;
        }

        public override string ToString()
        {
            return string.Format("Valid: {0}, Yesterday: {1} & Day before yesteday: {2}", isValid() ,Yesterday, DayBeforeYesterday);
        }

    }
}

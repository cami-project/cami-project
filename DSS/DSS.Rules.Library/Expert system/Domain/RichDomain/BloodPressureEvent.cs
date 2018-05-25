using System;
namespace DSS.Rules.Library.Domain
{
    public class BloodPressureEvent : IEvent
    {
        public string Owner { get; set; }
        public string Lang { get; set; }
        public string Timezone { get; set; }
        public DateTime Timestamp { get; set; }

        public int Diastolic { get; set; }
        public int Systolic { get; set; }

        public bool DiastolicInRange(int lower, int upper)
        {
            return Diastolic >= lower && Diastolic <= upper;
        }
        public bool SystolicInRange(int lower, int upper)
        {
            return Systolic >= lower && Systolic <= upper;
        }

        public override string ToString()
        {
            return string.Format("{0} - DPB {1} - SPB {2} ", Owner, Diastolic, Systolic);
        }
    }
}

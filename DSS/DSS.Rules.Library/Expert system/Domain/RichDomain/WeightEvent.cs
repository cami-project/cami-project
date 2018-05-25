using System;
namespace DSS.Rules.Library.Domain
{
    public class WeightEvent : IEvent
    {

        public float Value { get; set; }
        public float PreviousValue { get; set; }


        public string Owner { get; set; }
        public string Lang { get; set; }
        public string Timezone { get; set; }
        public DateTime Timestamp { get; set; }


        public bool IsDifferenceBiggerThan(float treshold)
        {
            return Math.Abs(PreviousValue - Value) > treshold;
        }

        public override string ToString()
        {
            return string.Format("KG: {0} : previous KG:  {1} for {2} at {3}", Value, PreviousValue, Owner, Timestamp); 
        }
    }
}

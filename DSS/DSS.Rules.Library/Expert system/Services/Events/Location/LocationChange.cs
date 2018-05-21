using System;

namespace DSS.Rules.Library
{
    public class LocationChange : IEvent
    {
        public string Current;
        public string Previous;

        public string Owner { get; set; }
        public string Lang { get; set; }
        public string Timezone { get; set; }
        public DateTime Timestamp { get; set; }

        public LocationChange(string userID, string previous, string current)
        {
            this.Owner = userID;
            this.Current = current;
            this.Previous = previous;
        }

        public LocationChange(Domain.MotionEvent e, string previous) 
        {
            this.Previous = previous;
            this.Owner = e.Owner;
            this.Current = e.Location;
        }

        public bool FromTo(string from, string to)
        {
            return from == this.Previous && to == this.Current;
        }

        public bool ToBedroom()
        {
            return this.Current == "BEDROOM";
        }


        public override string ToString()
        {
            return "From " + this.Previous + " to " + this.Current;
        }
    }
}

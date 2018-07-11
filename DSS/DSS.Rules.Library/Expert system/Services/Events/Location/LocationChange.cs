﻿using System;
using System.Collections.Generic;

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
        public IList<string> Caregivers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public LocationChange(IOwner owner, string previous, string current)
        {
            this.Owner = owner.Owner;
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

        public bool ToHallway()
        {
            return this.Current == "HALLWAY";
        }


        public override string ToString()
        {
            return "From " + this.Previous + " to " + this.Current;
        }
    }
}

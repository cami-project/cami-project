using System;
using System.Collections.Generic;

namespace DSS.Rules.Library.Domain
{
    public class MotionEvent : IEvent
    {
        public string Location { get; set; }

        public string Owner { get; set; }
        public string Lang { get; set; }
        public string Timezone { get; set; }
        public DateTime Timestamp { get; set; }
        public IList<string> Caregivers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        //maybe to have a JSON string store here??

    }
}

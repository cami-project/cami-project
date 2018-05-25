using System;
namespace DSS.Rules.Library.Domain
{
    public class FallEvent : IEvent
    {
        public DateTime Timestamp { get; set; }
        public string Owner { get; set; }
        public string Lang { get; set; }
        public string Timezone { get; set; }
    }
}

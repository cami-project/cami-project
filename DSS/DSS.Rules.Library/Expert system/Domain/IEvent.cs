using System;
namespace DSS.Rules.Library
{
    public interface IEvent
    {
        string Owner { get; set; }
        string Lang { get; set; }
        string Timezone { get; set; }

        DateTime Timestamp { get; set; }
    }
}

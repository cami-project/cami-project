using System;
namespace DSS.Rules.Library
{
    public interface IOwner 
    {
        string Owner { get; set; }
        string Lang { get; set; }
        string Timezone { get; set; }
    }

    public interface IEvent : IOwner
    {
        DateTime Timestamp { get; set; }
    }
}

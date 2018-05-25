using System;
namespace DSS.Rules.Library
{

    public interface IEvent : IOwner
    {
        DateTime Timestamp { get; set; }
    }
}

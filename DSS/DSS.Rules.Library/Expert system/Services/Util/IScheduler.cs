using System;
namespace DSS.Rules.Library
{
    public interface IScheduler : IIntervalInvoke
    {
        void Add(SheduledEvent e);
    }
}

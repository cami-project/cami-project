using System;
namespace DSS.Rules.Library
{
    public interface IScheduler
    {
        void Add(SheduledEvent e);
    }
}

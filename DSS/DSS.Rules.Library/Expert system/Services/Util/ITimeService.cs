using System;
namespace DSS.Rules.Library
{
    public interface ITimeService
    {
        bool HappenedInMorning(IEvent e);
        bool HappenedAtNight(IEvent e);
    }
}

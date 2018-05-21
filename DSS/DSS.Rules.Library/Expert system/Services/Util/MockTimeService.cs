using System;
namespace DSS.Rules.Library
{
    public class MockTimeService : ITimeService
    {
        private bool happenedAtNight;
        private bool happenedInMorning;

        public MockTimeService(bool isNight, bool isMorning)
        {
            this.happenedAtNight = isNight;
            this.happenedInMorning = isMorning;
        }

        public bool HappenedAtNight(IEvent e)
        {
            return this.happenedAtNight;
        }

        public bool HappenedInMorning(IEvent e)
        {
            return this.happenedInMorning;
        }
    }
}

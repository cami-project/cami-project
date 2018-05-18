using System;

namespace DSS.Rules.Library
{
    public class LocationState
    {
        public long TimeStampEnter;
        public long TimeStampMovement;

        public string Name;

        public DateTime TimeEnter;
        public DateTime TimeMovement;

        public string Owner;

        public LocationState(MotionEvent motionEvent)
        {

            Owner = motionEvent.getGateway();
            Name = motionEvent.getLocationName();
            TimeStampEnter = motionEvent.annotations.timestamp;

            TimeEnter = TimeService.UnixTimestampToDateTime(TimeStampEnter);
        }

        public override string ToString()
        {
            return this.Name + " " + this.Owner + ": " + TimeEnter.ToShortTimeString() + " - " + TimeMovement.ToShortTimeString();
        }
    }
}

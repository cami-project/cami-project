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
        public LocationState(Domain.MotionEvent e)
        {
            this.Owner = e.Owner;
            this.Name = e.Location;
            this.TimeMovement = e.Timestamp;
        }

        public override string ToString()
        {
            return this.Name + " " + this.Owner + ": " + TimeEnter.ToShortTimeString() + " - " + TimeMovement.ToShortTimeString();
        }
    }
}

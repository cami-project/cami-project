using System;

namespace DSS.Rules.Library
{

    public enum ActivityType 
    {
        Null, Movement, Fall, LowPulse, MightBeSleeping, ShedulingSleepingCheck,
        MightBeNightWandering,
        NightWanderingConfirmed
    }

    public class Activity
    {
        public readonly string Location;
        public readonly string Owner;
        public readonly string Description;
        private readonly string PartOfTheSystem;

        public DateTime Timestamp;
        readonly public ActivityType Type;


        //TODO: check how to get part of the system by refelection
        public Activity(string owner, ActivityType type, string description, string partOfTheSystem)
        {
            this.Owner = owner;
            this.Type = type;
            this.Description = description;
            this.PartOfTheSystem = partOfTheSystem;

            this.Location = "Unknown";

            this.Timestamp = DateTime.UtcNow;
        }

        public Activity(string owner, string location, DateTime timeStamp)
        {
            this.Owner = owner;
            this.Location = location;
            this.Timestamp = timeStamp;
        }

        public override string ToString()
        {
            return "Activity in " + Location + " for " + Owner + " at " + Timestamp.ToShortTimeString() + " Description: " + Description + " - Type: " + Type;
        }

        public bool NotBedroom()
        {
            return this.Location != "BEDROOM";
        }


        public bool IsType(ActivityType type) 
        {
            return this.Type == type;
        }


        public bool IsMovement()
        {
            return Type == ActivityType.Movement;

        }


    }
}

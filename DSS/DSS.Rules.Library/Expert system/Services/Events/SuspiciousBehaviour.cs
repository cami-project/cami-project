using System;
namespace DSS.Rules.Library
{
    public class SuspiciousBehaviour
    {
        private IInform inform;

        public SuspiciousBehaviour(IInform inform)
        {
            this.inform = inform;
        }

        public void ToLongInBathroom(LocationTimeSpent locationTime)
        {
            Console.WriteLine("Too long in the bathroom");
        }

        public void MightBeNightWandering(IEvent  activity) 
        {
            Console.WriteLine("Might be night wandering");

            inform.ActivityLog.ChangeAssumedState(activity, AssumedState.Awake);
            inform.ActivityLog.Log(new Activity(activity, ActivityType.MightBeNightWandering, "A night wandering might be detected", "SuspiciousBehaviour.NightWandering(IEvent)"));

            SheduleService.Add(new SheduledEvent(activity.Owner, SheduleService.Type.CheckForNightWandering, DateTime.UtcNow.AddHours(1)));
        }

        public void NightWanderingConfirmed(SheduledEvent sheduledEvent)
        {
            Console.WriteLine("Night wandering confirmed for the user: " + sheduledEvent.Owner);
            inform.ActivityLog.Log(new Activity(sheduledEvent, ActivityType.NightWanderingConfirmed, "Night wandering confirmed", "SuspiciousBehaviour.NightWanderingConfirmed(SheduledEvent)"));
        }
    }
}

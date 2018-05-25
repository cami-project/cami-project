using System;
using System.Collections.Generic;
using System.Linq;

namespace DSS.Rules.Library
{
    public class ActivityLog : IActivityLog
    {
        private Dictionary<string, List<Activity>> Logger = new Dictionary<string, List<Activity>>();
        public Action<Activity> ActivityRuleHandler { get; set; }


        public void Log(Activity activity)
        {
            if(!Logger.ContainsKey(activity.Owner))
            {
                Logger.Add(activity.Owner, new List<Activity>());
            }

            Logger[activity.Owner].Add(activity);

            Console.WriteLine("Activity added: " + activity);
            ActivityRuleHandler?.Invoke(activity);
        }

        public bool EventOfTypeHappenedAfter(IOwner owner, ActivityType before, ActivityType after)
        {

            if(!Logger.ContainsKey(owner.Owner))
            {
                Console.WriteLine("The user of ID: {0} does not exist in the acitiviy logger (if you see this it is probably a BUG )", owner);
                return false;
            }

            var indexOfBefore = Logger[owner.Owner].FindLastIndex(x => x.Type == before);

            while (indexOfBefore < Logger[owner.Owner].Count)
            {
                if (after == Logger[owner.Owner][indexOfBefore].Type)
                    return true;

                indexOfBefore++;
            }
            return false;
        }

        public bool EventOfTypeHappened(IOwner owner, ActivityType before, int minBack )
        {
            if (!Logger.ContainsKey(owner.Owner)) throw new Exception("The user of ID: " + owner + "does not exist in the acitiviy logger(if you see this it is probably a BUG )");

            var activity = Logger[owner.Owner].FindLast(x => x.Type == before);

            if (activity == null)
                return false;
            
            return activity.Timestamp.AddMinutes(minBack) >= DateTime.UtcNow;
        }

        public bool DidNotHappenAfter(IOwner owner, ActivityType after, ActivityType didNotHappen)
        {

            Console.WriteLine(after.ToString() + " - " + didNotHappen.ToString());

            if (!Logger.ContainsKey(owner.Owner)) throw new Exception("The user of ID: " + owner + "does not exist in the acitiviy logger(if you see this it is probably a BUG )");

            var indexOfAfter = Logger[owner.Owner].FindLastIndex(x => x.Type == after);

            if(indexOfAfter == -1) 
            {
                Console.WriteLine("Cannot find the specified acitivity type");
                return false;
            }

            while (indexOfAfter < Logger[owner.Owner].Count)
            {
                if (didNotHappen == Logger[owner.Owner][indexOfAfter].Type)
                    return false;

                indexOfAfter++;
            }

            return true;
        }

        public int TimeSince(IEvent e, ActivityType type)
        {
            return (e.Timestamp - Logger[e.Owner].FindLast(x => x.Type == type).Timestamp).Minutes;
        }

        public ActivityType GetLastActivityType(IOwner owner)
        {
            return Logger[owner.Owner].Last().Type;
        }

        //ASSUMED STATE
        private Dictionary<string, AssumedState> assumedState = new Dictionary<string, AssumedState>();

        public void ChangeAssumedState(IOwner owner, AssumedState state)
        {
            assumedState[owner.Owner] = state;
        }

        public AssumedState GetAssumedState(IOwner owner)
        {
            if (assumedState.ContainsKey(owner.Owner))
                return assumedState[owner.Owner];
            
            return AssumedState.Null;
        }

        public bool IsAssumedState(IOwner owner, AssumedState state)
        {
            return assumedState[owner.Owner] == state;
        }
    }
}

using System;
using System.Collections.Generic;

namespace DSS.Rules.Library
{
    public class ActivityLog : IActivityLog
    {
        private Dictionary<string, List<Activity>> Logger = new Dictionary<string, List<Activity>>();

        public void Log(Activity activity)
        {
            if(!Logger.ContainsKey(activity.Owner))
            {
                Logger.Add(activity.Owner, new List<Activity>());
            }

            Logger[activity.Owner].Add(activity);

            Console.WriteLine("Activity added: " + activity);
        }

        public bool EventOfTypeHappenedAfter(string owner ,ActivityType before, ActivityType after)
        {

            if(!Logger.ContainsKey(owner))
            {
                Console.WriteLine("The user of ID: {0} does not exist in the acitiviy logger (if you see this it is probably a BUG )");
                return false;
            }

            var indexOfBefore = Logger[owner].FindLastIndex(x => x.Type == before);


            for (;indexOfBefore < Logger[owner].Count ; indexOfBefore++)
            {
                if (after == Logger[owner][indexOfBefore].Type)
                    return true;
            }

            return false;
        }

    }
}

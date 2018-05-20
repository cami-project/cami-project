﻿using System;
using System.Collections.Generic;

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

        public bool EventOfTypeHappenedAfter(string owner, ActivityType before, ActivityType after)
        {

            if(!Logger.ContainsKey(owner))
            {
                Console.WriteLine("The user of ID: {0} does not exist in the acitiviy logger (if you see this it is probably a BUG )", owner);
                return false;
            }

            var indexOfBefore = Logger[owner].FindLastIndex(x => x.Type == before);

            while (indexOfBefore < Logger[owner].Count)
            {
                if (after == Logger[owner][indexOfBefore].Type)
                    return true;

                indexOfBefore++;
            }
            return false;
        }

        public bool EventOfTypeHappened(string owner, ActivityType before, int minBack )
        {
            if (!Logger.ContainsKey(owner)) throw new Exception("The user of ID: " + owner + "does not exist in the acitiviy logger(if you see this it is probably a BUG )");

            var activity = Logger[owner].FindLast(x => x.Type == before);

            if (activity == null)
                return false;

            return activity.Timestamp.AddMinutes(minBack) >= DateTime.UtcNow;
        }
    }
}

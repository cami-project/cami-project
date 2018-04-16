using System;
using System.Collections.Generic;
using System.Linq;

namespace DSS.Rules.Library
{

    public class InternalEvent 
    {
        public string Type;
        public string Name;

        public InternalEvent(string type, string name)
        {
            this.Name = name;
            this.Type = type;
        }
    }

    public class InMemoryDB
    {

        private static Dictionary<string, object> collections = new Dictionary<string, object>();
        private static Dictionary<string, List<State>> history = new Dictionary<string, List<State>>();


        private static Dictionary<string, List<InternalEvent>> reminders = new Dictionary<string, List<InternalEvent>>();

        public static void AddReminder(string id, InternalEvent reminder) 
        {
            if(!reminders.ContainsKey(id)) 
            {
                reminders.Add(id, new List<InternalEvent>()); 
            }

            reminders[id].Add(reminder);
        }

        public static bool ReminderExist(string id, InternalEvent reminder)
        {
            if(reminders.ContainsKey(id))
            {
                return reminders[id].FirstOrDefault(x => x.Name == reminder.Name && x.Type == reminder.Type) != null;    
            }
            return false;
        }


        public static bool WeightReminderSent(string id)
        {
            return ReminderExist(id, new InternalEvent("SENT", "WEIGHT"));
        }

        public static bool Exists(string key) {
            
            return collections.ContainsKey(key);
        }

        public static void Push(string key, object val) {

            collections.Add(key, val);

        }

        public static T Get<T>(string key){

            return (T)collections[key];

        }
        public static void Remove(string key) {

            collections.Remove(key);
        }

        public static void AddHistory(string id, State state)
        {

            if(!history.ContainsKey(id))
            {
                history.Add(id, new List<State>());    
            }

            history[id].Add(state);
            Console.WriteLine("State: " + state.Name + " added for " + id);
        }

        //This should be invoked at midnight 
        public static void CleanHistory() 
        {
            history = new Dictionary<string, List<State>>();
            Console.WriteLine("History cleaned");
        }

    }
}

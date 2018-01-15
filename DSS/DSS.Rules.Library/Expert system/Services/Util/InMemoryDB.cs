using System;
using System.Collections.Generic;

namespace DSS.Rules.Library
{
    public class InMemoryDB
    {

        private static Dictionary<string, object> collections = new Dictionary<string, object>();


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
    }
}

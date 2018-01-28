using System;
using System.Linq;

namespace DSS.Rules.Library
{
    public class Inform
    {

        public RMQ.StoreAPI storeAPI;
        public RMQ.INS.InsertionAPI insertionAPI;


        public Inform(string storeURL, string insertionURL)
        {
            storeAPI = new RMQ.StoreAPI(storeURL);
            insertionAPI = new RMQ.INS.InsertionAPI(insertionURL);
        }

        public void Caregivers(string enduserURI, string type, string severity, string msg, string desc, bool notify = true)
        {
            var caregivers = storeAPI.GetCaregivers(enduserURI);

            foreach (string caregiverURIPath in caregivers)
            {
                storeAPI.PushJournalEntry(caregiverURIPath, type, severity, msg, desc);
                if(notify)
                {
					insertionAPI.InsertPushNotification(msg, GetIdFromURI(caregiverURIPath));
                }
            }

        }
        public void User(string enduserURI, string type, string severity, string msg, string desc)
        {

            storeAPI.PushJournalEntry(enduserURI, type, severity, msg, desc);
            insertionAPI.InsertPushNotification(msg, GetIdFromURI(enduserURI));

        }

        public int GetIdFromURI(string uri)
        {
            return Int32.Parse(uri.TrimEnd('/').Split('/').Last());
        }

        public string URIfromID(int id, string path ) 
        {
            return string.Format("/api/v1/{0}/{1}/", path, id);
        }

    
    }
}

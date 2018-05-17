using System;
using System.Linq;
using DSS.RMQ;

namespace DSS.Rules.Library
{
    public class Inform
    {

        public IStoreAPI storeAPI;
        public IInsertionAPI insertionAPI;


        public Inform(IStoreAPI storeAPI, IInsertionAPI insertionAPI)
        {
            this.storeAPI = storeAPI;
            this.insertionAPI = insertionAPI;
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

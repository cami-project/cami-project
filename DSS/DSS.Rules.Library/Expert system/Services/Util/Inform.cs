using System;
using System.Linq;
using DSS.RMQ;

namespace DSS.Rules.Library
{
    public class Inform : IInform
    {

        public IStoreAPI StoreAPI { get; set; }
        public IInsertionAPI InsertionAPI { get; set; }
        public IActivityLog ActivityLog { get; set; }

   
        public Inform(IStoreAPI storeAPI, IInsertionAPI insertionAPI, IActivityLog activityLog)
        {
            this.StoreAPI = storeAPI;
            this.InsertionAPI = insertionAPI;
            this.ActivityLog = activityLog;
        }

        public void Caregivers(string enduserURI, string type, string severity, string msg, string desc, bool notify = true)
        {
            var caregivers = StoreAPI.GetCaregivers(enduserURI);

            foreach (string caregiverURIPath in caregivers)
            {
                StoreAPI.PushJournalEntry(caregiverURIPath, type, severity, msg, desc);
                if(notify)
                {
					InsertionAPI.InsertPushNotification(msg, GetIdFromURI(caregiverURIPath));
                }
            }

        }
        public void User(string enduserURI, string type, string severity, string msg, string desc)
        {
            StoreAPI.PushJournalEntry(enduserURI, type, severity, msg, desc);
            InsertionAPI.InsertPushNotification(msg, GetIdFromURI(enduserURI));
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

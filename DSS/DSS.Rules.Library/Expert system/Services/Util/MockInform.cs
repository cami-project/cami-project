using System;
using DSS.RMQ;

namespace DSS.Rules.Library
{
    public class MockInform : IInform
    {
        public IStoreAPI StoreAPI { get ; set; }
        public IInsertionAPI InsertionAPI { get ; set ; }
        public IActivityLog ActivityLog { get; set; }

        public MockInform(IStoreAPI storeAPI, IInsertionAPI insertionAPI, IActivityLog activityLog)
        {
            this.StoreAPI = storeAPI;
            this.InsertionAPI = insertionAPI;
            this.ActivityLog = activityLog;
        }

        public void Caregivers(string enduserURI, string type, string severity, string msg, string desc, bool notify = true)
        {
            Console.WriteLine("Mock: Inform.Caregivers called with parameters: {0}, {1}, {2}, {3}, {4}, {5}", enduserURI, type, severity, msg, desc, notify);
        }

        public int GetIdFromURI(string uri)
        {
            throw new NotImplementedException();
        }

        public string URIfromID(int id, string path)
        {
            throw new NotImplementedException();
        }

        public void User(string enduserURI, string type, string severity, string msg, string desc)
        {
            throw new NotImplementedException();
        }
    }
}

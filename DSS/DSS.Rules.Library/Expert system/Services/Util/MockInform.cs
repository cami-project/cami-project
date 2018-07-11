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

        public void NotifyOwner(IOwner owner, NotificationType type, Severity severity, string key)
        {
            var t = type.ToString().ToLower();
            var s = severity.ToString().ToLower();

            //Console.WriteLine("LANGIC :" + owner.Lang);

            string msg = "Message not available: ";

            try
            {
                msg = Loc.Msg(key, owner.Lang, Loc.USR);
            }
            catch (Exception ex)
            {
                msg += " : " + ex.Message;
            }

            string desc = "Description not available";
            try
            {
                desc = Loc.Des(key, owner.Lang, Loc.USR);
            }
            catch (Exception ex)
            {
                desc += " : " + ex.Message;
            }


            Console.WriteLine("[Notifiy owner]: {0} - {1} - {2} - {3} - {4} - {5} ", owner, t, s, key, msg, desc);
        }

        public void NotifyCaregivers(IOwner owner, NotificationType type, Severity severity, string msg, string desc, bool pushNotification = true)
        {
            throw new NotImplementedException();
        }
    }
}

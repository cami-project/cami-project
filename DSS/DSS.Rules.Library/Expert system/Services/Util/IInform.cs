using DSS.RMQ;

namespace DSS.Rules.Library
{
    public interface IInform
    {
        IStoreAPI StoreAPI { get; set; }
        IInsertionAPI InsertionAPI { get; set; }
        IActivityLog ActivityLog { get; set; }

        void Caregivers(string enduserURI, string type, string severity, string msg, string desc, bool notify = true);
        int GetIdFromURI(string uri);
        string URIfromID(int id, string path);
        void User(string enduserURI, string type, string severity, string msg, string desc);
    }
}
namespace DSS.RMQ
{
    public interface IInsertionAPI
    {
        void InsertEvent(string json);
        void InsertPushNotification(string json);
        void InsertPushNotification(string msg, int userId);
    }
}
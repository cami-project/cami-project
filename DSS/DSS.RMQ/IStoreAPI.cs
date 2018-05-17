using System;

namespace DSS.RMQ
{
    public interface IStoreAPI
    {
        bool AreLastNHeartRateCritical(int n, int low, int high);
        bool CheckForMeasuremntInLastNMinutes(string type, int min, string userId);
        JournalEntryResponse GetBathroomVisitsForLastDays(int numOfDays, int user);
        dynamic GetCaregivers(string userURIPath);
        int GetIdFromURI(string uri);
        JournalEntry GetJournalEntryById(string id);
        string GetLang(string userURI);
        float GetLatestWeightMeasurement(int userId);
        dynamic GetUserData(string userURIPath);
        Tuple<string, string> GetUserLocale(string userURIPath, int userID);
        string GetUserOfGateway(string gatewayURIPath);
        int GetUserStepCount(string userURIPath, long startTs, long endTs);
        void PatchJournalEntry(string id, bool ack);
        JournalEntry PushJournalEntry(string user_uri, string notification_type, string severity, string msg, string desc, long reference_id = 0);
        void PushMeasurement(string json);
    }
}
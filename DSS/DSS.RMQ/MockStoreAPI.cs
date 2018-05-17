using System;
using System.Collections.Generic;

namespace DSS.RMQ
{
    public class MockStoreAPI : IStoreAPI
    {

        public JournalEntryResponse GetBathroomVisitsForLastDays(int numOfDays, int userID)
        {
            var journalEntryReponse = new JournalEntryResponse();

            journalEntryReponse.Objects = new List<JournalEntryItem>();

            journalEntryReponse.Objects.Add(new JournalEntryItem() { Timestamp = "1526428800" });
            journalEntryReponse.Objects.Add(new JournalEntryItem() { Timestamp = "1526428800" });
            journalEntryReponse.Objects.Add(new JournalEntryItem() { Timestamp = "1526342400" });
            journalEntryReponse.Objects.Add(new JournalEntryItem() { Timestamp = "1526342400" });
            journalEntryReponse.Objects.Add(new JournalEntryItem() { Timestamp = "1526303975" });
            journalEntryReponse.Objects.Add(new JournalEntryItem() { Timestamp = "1526217575" });
            journalEntryReponse.Objects.Add(new JournalEntryItem() { Timestamp = "1526131175" });
            journalEntryReponse.Objects.Add(new JournalEntryItem() { Timestamp = "1526044775" });
            journalEntryReponse.Objects.Add(new JournalEntryItem() { Timestamp = "1525958375" });


            return journalEntryReponse;
        }

        //NOT IMPLEMENTED YET


        public bool AreLastNHeartRateCritical(int n, int low, int high)
        {
            throw new NotImplementedException();
        }

        public bool CheckForMeasuremntInLastNMinutes(string type, int min, string userId)
        {
            throw new NotImplementedException();
        }

       

        public dynamic GetCaregivers(string userURIPath)
        {
            throw new NotImplementedException();
        }

        public int GetIdFromURI(string uri)
        {
            throw new NotImplementedException();
        }

        public JournalEntry GetJournalEntryById(string id)
        {
            throw new NotImplementedException();
        }

        public string GetLang(string userURI)
        {
            throw new NotImplementedException();
        }

        public float GetLatestWeightMeasurement(int userId)
        {
            throw new NotImplementedException();
        }

        public dynamic GetUserData(string userURIPath)
        {
            throw new NotImplementedException();
        }

        public Tuple<string, string> GetUserLocale(string userURIPath, int userID)
        {
            throw new NotImplementedException();
        }

        public string GetUserOfGateway(string gatewayURIPath)
        {
            throw new NotImplementedException();
        }

        public int GetUserStepCount(string userURIPath, long startTs, long endTs)
        {
            throw new NotImplementedException();
        }

        public void PatchJournalEntry(string id, bool ack)
        {
            throw new NotImplementedException();
        }

        public JournalEntry PushJournalEntry(string user_uri, string notification_type, string severity, string msg, string desc, long reference_id = 0)
        {
            throw new NotImplementedException();
        }

        public void PushMeasurement(string json)
        {
            throw new NotImplementedException();
        }
    }
}

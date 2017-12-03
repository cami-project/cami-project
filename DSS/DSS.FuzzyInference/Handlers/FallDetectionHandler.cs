using System;
using System.Linq;
using DSS.Delegate;
using DSS.RMQ;
using Newtonsoft.Json;

namespace DSS.FuzzyInference
{
    public class FallDetectionHandler : IRouterHandler
    {
        private RMQ.INS.InsertionAPI insertionAPI;
        private StoreAPI storeAPI;


        public FallDetectionHandler()
        {
            insertionAPI = new RMQ.INS.InsertionAPI("http://cami-insertion:8010/api/v1/insertion");
            storeAPI = new StoreAPI("http://cami-store:8008");
        }

        public void Handle(string json)
        {
            var fall = JsonConvert.DeserializeObject<dynamic>(json);

            if (fall.category.ToString() == "FALL_DETECTION")
            {
                Console.WriteLine("FALL detected!");

                var path = "/api/v1/user/" + fall.content["user"]["id"] + "/";
                Console.WriteLine(path);

                var lang = storeAPI.GetLang(path);

                //Console.WriteLine(path);

                InformCaregivers(path, "fall", "high", 
                                 Loc.Get(lang, Loc.MSG, Loc.FALL_DETECTED, Loc.CAREGVR), 
                                 Loc.Get(lang, Loc.DES, Loc.FALL_DETECTED, Loc.CAREGVR) 
                    
                                );

            }
        }


        private void InformCaregivers(string enduserURI, string type, string severity, string msg, string desc)
        {
            Console.WriteLine("[Reminder handler] Informing caregiver: " + enduserURI + " of: " + msg);
            var caregivers = storeAPI.GetCaregivers(enduserURI);

            foreach (string caregiverURIPath in caregivers)
            {
                int caregiverID = GetIdFromURI(caregiverURIPath);

                storeAPI.PushJournalEntry(caregiverURIPath, type, severity, msg, desc);
                insertionAPI.InsertPushNotification(msg, caregiverID);
            }

        }
        private int GetIdFromURI(string uri)
        {
            var idStr = uri.TrimEnd('/').Split('/').Last();
            return Int32.Parse(idStr);
        }
    }
}

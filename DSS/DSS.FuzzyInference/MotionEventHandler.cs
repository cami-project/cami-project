using DSS.Delegate;
using DSS.RMQ;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.FuzzyInference
{
    public class MotionEventHandler : IRouterHandler
    {
        private StoreAPI storeAPI;
        private RMQ.INS.InsertionAPI insertionAPI;

        /**
         * Map that stores the last activation timestamp of the motion sensor for each user uri
         */
        private Dictionary<string, long> lastActivationMap;

        public string Name => "EVENT";
        
        public MotionEventHandler()
        {
            storeAPI = new StoreAPI("http://cami-store:8008");
            insertionAPI = new RMQ.INS.InsertionAPI("http://cami-insertion:8010/api/v1/insertion");

            lastActivationMap = new Dictionary<string, long>();
        }

        public void Handle(string json)
        {
            Console.WriteLine("MOTION event handler invoked ...");
            try {
                var eventObj = JsonConvert.DeserializeObject<Event>(json);
                Console.WriteLine("[MotionEventHandler] Handling eventObj: " + eventObj);

                if (eventObj.category.ToLower() == "user_environment")
                {
                    // if we are dealing with a presence sensor
                    if (eventObj.content.name == "presence")
                    {
                        // see if it is a sensor activation
                        if ((bool)eventObj.content.val["alarm_motion"] == true)
                        {
                            // retrieve the gateway and sensor URI from the source annotations
                            var gatewayURIPath = eventObj.annotations.source["gateway"];
                            var deviceURIPath = eventObj.annotations.source["sensor"];

                            // make a call to the store API to get the user from the gateway
                            var userURIPath = storeAPI.GetUserOfGateway(gatewayURIPath);

                            if (userURIPath != null)
                            {
                                int userID = GetIdFromURI(userURIPath);
                                Tuple<string, string> userLocales = storeAPI.GetUserLocale(userURIPath, userID);

                                if (userLocales != null)
                                {
                                    // retrieve timestamp from annotations
                                    long timestamp = eventObj.annotations.timestamp;

                                    // get localized datetime
                                    TimeZoneInfo localTz = TimeZoneInfo.FindSystemTimeZoneById(userLocales.Item2);
                                    DateTime dtime = UnixTimeStampToDateTime(timestamp, localTz);

                                    // get morning limits
                                    Tuple<DateTime, DateTime> morningLimits = getMorningLimits(localTz);

                                    // check if current dtime is within limits
                                    if (dtime >= morningLimits.Item1 || dtime <= morningLimits.Item2)
                                    {
                                        if (lastActivationMap.ContainsKey(userURIPath))
                                        {
                                            long lastTs = lastActivationMap[userURIPath];
                                            DateTime lastDt = UnixTimeStampToDateTime(lastTs, localTz);

                                            // if the lastDt is outside of the morning limits
                                            if (lastDt < morningLimits.Item1)
                                            {
                                                Console.WriteLine("[MotionEventHandler] Identified first activation of motion sensor in morning at : " + dtime.ToString());
                                                SendBPMeasurementNotification(userURIPath);
                                            }

                                            lastActivationMap[userURIPath] = timestamp;
                                        }
                                        else
                                        {
                                            // if this is the first activation ever within morning limits
                                            Console.WriteLine("[MotionEventHandler] First ever activation of motion sensor in morning at : " + dtime.ToString());
                                            SendBPMeasurementNotification(userURIPath);

                                            lastActivationMap[userURIPath] = timestamp;
                                        }
                                    }
                                    else
                                    {
                                        // just mark as latest activation
                                        Console.WriteLine("[MotionEventHandler] Motion event not within morning limits " + dtime.ToString());
                                        lastActivationMap.Add(userURIPath, timestamp);
                                    }

                                }
                                else
                                {
                                    Console.WriteLine("[MotionEventHandler] Insufficient locales information in enduser profile endpoint for user: " + userURIPath);
                                }
                            }
                            else
                            {
                                Console.WriteLine("[MotionEventHandler] Skipping motion event handling since no user can be retrieved for gateway: " + gatewayURIPath);
                            }
                        }
                        else
                        {
                            Console.WriteLine("[MotionEventHandler] The motion sensor has not been triggered: " + eventObj.content.val["alarm_motion"]);
                        }
                    }

                    Console.WriteLine("[MotionEventHandler] Handling eventObj of type: " + eventObj.content.name);
                }

                Console.WriteLine("[MotionEventHandler] Handling eventObj of category: " + eventObj.category.ToLower());
            }
            catch (JsonException ex)
            {
                Console.WriteLine(ex);
            }
        }


        private void SendBPMeasurementNotification(string userURIPath)
        {
            string enduser_msg = "Time for your morning blood pressure measurement!";
            string enduser_desc = "Please take your blood pressure. Follow the instructions from the web interface on how to do so.";

            string caregiver_msg = "Reminder for morning blood pressure measurement sent!";
            string caregiver_desc = "Please check on your loved one to see that he took the recommended BP measurement.";

            string notification_type = "medication";

            // get the user data to retrieve caregiver uri as well
            dynamic userData = storeAPI.GetUserData(userURIPath);

            if (userData != null)
            {
                // send notification for end user
                int enduserID = GetIdFromURI(userURIPath);
                JournalEntry endUserJournalEntry = storeAPI.PushJournalEntry(userURIPath, notification_type, "low", enduser_msg, enduser_desc);
                insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = enduser_msg, user_id = enduserID }));

                // send notification to all caregivers
                List<int> caregiverJournalEntryIDs = new List<int>();
                dynamic profile = userData["enduser_profile"];
                if (((Dictionary<string, dynamic>) profile).ContainsKey("caregivers"))
                {
                    foreach (var caregiverURIPath in profile["caregivers"]) {
                        int caregiverID = GetIdFromURI(caregiverURIPath);

                        var caregiverJournalEntry = storeAPI.PushJournalEntry(caregiverURIPath, notification_type, "low", caregiver_msg, caregiver_desc);
                        insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = caregiver_msg, user_id = caregiverID}));

                        caregiverJournalEntryIDs.Add(caregiverJournalEntry.id);
                    }
                }

                // generate reminder event
                var reminderEvent = new Event() {
                    category = "USER_NOTIFICATIONS",
                    content = new Content() {
                        uuid = Guid.NewGuid().ToString(),
                        name = "reminder_sent",
                        value_type = "complex",
                        val = new Dictionary<string, dynamic>()
                        {
                             { "user", new Dictionary<string, int>() { {"id", enduserID } } },
                             { "journal", new Dictionary<string, dynamic>() {
                                 { "id_enduser", endUserJournalEntry.id },
                                 { "id_caregivers", caregiverJournalEntryIDs.ToArray<int>()}
                             } },
                        }
                    },
                    annotations = new Annotations()
                    {
                        timestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                        source = "DSS"
                    }
                };

                Console.WriteLine("[MotionEventHandler] Inserting new reminderEvent: " + reminderEvent);
                insertionAPI.InsertEvent(JsonConvert.SerializeObject(reminderEvent));
            } 
            else
            {
                Console.WriteLine("[MotionEventHandler] Error - no information available on user with uri: " + userURIPath + ". Cannot send notification to end user and caregivers.");
            }
        }


        private DateTime UnixTimeStampToDateTime(long unixTimeStamp, TimeZoneInfo tzInfo)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);

            dtDateTime = TimeZoneInfo.ConvertTime(dtDateTime, TimeZoneInfo.Local, tzInfo);
            return dtDateTime;
        }

        private int GetIdFromURI(string uri)
        {
            string idStr = uri.TrimEnd('/').Split('/').Last();

            int id = Int32.Parse(idStr);
            return id;
        }

        private Tuple<DateTime, DateTime> getMorningLimits(TimeZoneInfo tzInfo)
        {
            DateTime morningStart = DateTime.UtcNow;
            DateTime morningEnd = DateTime.UtcNow;

            morningStart = morningStart.Date.Add(new TimeSpan(6, 0, 0));
            morningEnd = morningEnd.Date.Add(new TimeSpan(11, 0, 0));

            morningStart = TimeZoneInfo.ConvertTime(morningStart, TimeZoneInfo.Local, tzInfo);
            morningEnd = TimeZoneInfo.ConvertTime(morningEnd, TimeZoneInfo.Local, tzInfo);

            return new Tuple<DateTime, DateTime>(morningStart, morningEnd);
        }
        
    }
}

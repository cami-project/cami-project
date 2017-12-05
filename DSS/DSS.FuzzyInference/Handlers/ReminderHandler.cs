using System;
using System.Collections.Generic;
using DSS.Delegate;
using System.Linq;
using DSS.RMQ;
using Newtonsoft.Json;
using librato4net;

namespace DSS.FuzzyInference
{
    public class ReminderHandler : IRouterHandler
    {
        private RMQ.INS.InsertionAPI insertionAPI;
        private StoreAPI storeAPI;
        private Dictionary<string, dynamic> userReminderMap;
        private Dictionary<string, int []> userCareGiversMap;

        private Dictionary<string, dynamic> userActiveExerciseMap;



       // private const int WAIT_MS = 60 * 1000;

        private const int WAIT_MS = 6 * 60 * 1000;

        public ReminderHandler()
        {
            insertionAPI = new RMQ.INS.InsertionAPI("http://cami-insertion:8010/api/v1/insertion");
            storeAPI = new StoreAPI("http://cami-store:8008");
            //storeAPI = new StoreAPI("http://141.85.241.224:8008");
            //insertionAPI = new RMQ.INS.InsertionAPI("http://cami-insertion:8010/api/v1/insertion");


            userReminderMap = new Dictionary<string, dynamic>();
            userActiveExerciseMap = new Dictionary<string, dynamic>();



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

        private void InformUser(string enduserURI, string type, string severity, string msg, string desc)
        {
            Console.WriteLine("[Reminder handler] Informing enduser: " + enduserURI + " of: " + msg);
            storeAPI.PushJournalEntry(enduserURI, type, severity, msg, desc);
            insertionAPI.InsertPushNotification(msg, GetIdFromURI(enduserURI));

        }
        private int GetIdFromURI(string uri)
        {
            var idStr = uri.TrimEnd('/').Split('/').Last();
            return Int32.Parse(idStr);
        }

        public void Handle(string json)
        {
            
            Console.WriteLine("Reminder invoked...");

            var reminder = JsonConvert.DeserializeObject<dynamic>(json);

            if(reminder.category.ToString().ToLower() == "user_notifications") {

                var userIdStr = reminder.content.value.user.id.ToString();
                var userURIPath = "/api/v1/user/" + userIdStr + "/";
                var key = userURIPath;

                var LANG = storeAPI.GetLang(userURIPath);

                if (reminder.content.name == "exercise_started")
                {
                    key = userURIPath + "_exercise";
                    Console.WriteLine("[ReminderHandler] Exercise started");
                    Console.WriteLine("[ReminderHandler] userActiveExcercise map key: " + key);

                    userActiveExerciseMap[key] = reminder;
                    string exerciseType = reminder.content.value.exercise_type.ToString();

                    InformCaregivers(userURIPath, "exercise", "none", Loc.Get(LANG, Loc.MSG, Loc.EXERCISE_STARTED, Loc.CAREGVR), string.Format(Loc.Get(LANG, Loc.DES, Loc.EXERCISE_STARTED, Loc.CAREGVR), exerciseType));
                    return;
                }
                else if (reminder.content.name == "exercise_ended")
                {
                    key = userURIPath + "_exercise";
                    Console.WriteLine("[ReminderHandler] Exercise ended");
                    Console.WriteLine("[ReminderHandler] userActiveExcercise map key: " + key);

                    if (userActiveExerciseMap.ContainsKey(key) && reminder.content.value.session_uuid == userActiveExerciseMap[key].content.value.session_uuid)
                    {
                        
                        Console.WriteLine("Exercise ended is in the active map");

                        float score = float.Parse(reminder.content.value.score.ToString());
                        string exerciseType = reminder.content.value.exercise_type.ToString();

                        InformCaregivers(userURIPath,
                                         "exercise",
                                         "none",
                                         Loc.Get(LANG, Loc.MSG, Loc.EXERCISE_ENDED, Loc.CAREGVR),
                                         string.Format(Loc.Get(LANG, Loc.DES, Loc.EXERCISE_ENDED, Loc.CAREGVR), exerciseType, score));



                        var desc = Loc.EXERCISE_ENDED_HIGH;

                        if (score <= 30)
                            desc = Loc.EXERCISE_ENDED_LOW;
                        else if (score > 30 && score <= 60)
                            desc = Loc.EXERCISE_ENDED_MID;


                        InformUser(userURIPath, "exercise", "low", Loc.Get(LANG, Loc.MSG, Loc.EXERCISE_ENDED_LOW, Loc.USR), Loc.Get(LANG, Loc.DES, desc, Loc.USR));
                        userActiveExerciseMap.Remove(key);
                    }
                    return;
                }

                //key = userURIPath + reminder.type;

                if (reminder.content.name == "reminder_sent")
                {

                    Console.WriteLine("[reminder_handler] reminder sent entered");
                    var journalId = reminder.content.value.journal.id_enduser.ToString();
                    var journalEntry = storeAPI.GetJournalEntryById(journalId);

                    Console.WriteLine("[reminder_handler] Sent journal entry message for reminder: " + journalEntry.message.ToString());

                    Console.WriteLine("Type: " + journalEntry.type.ToString());

                    var msg = new List<string>(journalEntry.message.ToString().Split(' '));
                    var des = new List<string>(journalEntry.description.ToString().Split(' '));

                    msg.AddRange(des);

                    Console.WriteLine("Contains blood or presure " + (msg.Contains("blood") || msg.Contains("pressure")));
                    Console.WriteLine("Contains weight: " + msg.Contains("weight"));

                    // set the key
                    if (msg.Contains("blood") || msg.Contains("pressure")) {
                        key = userURIPath + "_blood_pressure";
                    }
                    else if msg.Contains("weight") {
                        key = userURIPath + "_weight";
                    }
                    Console.WriteLine("[ReminderHandler] userReminder map key: " + key);


                    if (userReminderMap.ContainsKey(key))
                    {
                        userReminderMap.Remove(key);
                    }

                    userReminderMap.Add(key, reminder);

                    var aTimer = new System.Timers.Timer(WAIT_MS);
                    aTimer.AutoReset = false;
                    aTimer.Start();
                    aTimer.Elapsed += (x, y) =>
                    {
                        if(userReminderMap.ContainsKey(key)) {
                            if (msg.Contains("blood") || msg.Contains("pressure"))
                            {
                                Console.WriteLine("Blood pressure measured wasn't ack");

                                InformCaregivers(userURIPath, "heart", "high",
                                                 "The person under your care ignored the blood pressure measurements reminder.",
                                                 "Please take action and call to remind them of the measurement.");

                                userReminderMap.Remove(key);
                            }
                            else if(msg.Contains("weight")){

                                InformCaregivers(userURIPath, "weight", "high",
                                                   "The person under your care ignored the weight measurements reminder.",
                                                   "Please take action and call to remind them of the measurement.");

                                userReminderMap.Remove(key);
                            }
                        }
                    };

                }
                else if (reminder.content.name == "reminder_acknowledged")
                {
                    
                    var ack = reminder.content.value.ack.ToString() == "ok" ? true : false;
                    var journalId = reminder.content.value.journal.id.ToString();
                    var journalEntry = storeAPI.GetJournalEntryById(journalId);

                    Console.WriteLine(journalId);
                    storeAPI.PatchJournalEntry(journalId, ack);

                    var msg = new List<string>(journalEntry.message.ToString().Split(' '));
                    var des = new List<string>(journalEntry.description.ToString().Split(' '));

                    msg.AddRange(des);

                    Console.WriteLine("Contains blood or presure " + ( msg.Contains("blood") || msg.Contains("pressure")));
                    Console.WriteLine("Contains weight: " + msg.Contains("weight"));

                    if (ack)
                    {
                        MetricsPublisher.Current.Increment("cami.event.reminder.ack", 1);

                        Console.WriteLine("Reminder acknowledged");

                        //Do this just in case it's possible to check if user did something for example 
                        //there is a new value in weight measurements and ignore if it's not possible 
                        //for example medication

                        Console.WriteLine("Type: " + journalEntry.type.ToString());
                        Console.WriteLine("[reminder_handler] Acknowledged journal entry message: " + journalEntry.message.ToString());

                        //var expectedMessage = Loc.Get(LANG, Loc.MSG, Loc.REMINDER_SENT, Loc.USR);

                        if(msg.Contains("blood") || msg.Contains("pressure")) {

                            Console.WriteLine("ACK blood pressure measurement!");

                            // set key
                            key = userURIPath + "_blood_pressure";

                            var aTimer = new System.Timers.Timer(WAIT_MS);
                            aTimer.AutoReset = false;
                            aTimer.Start();
                            aTimer.Elapsed += (x, y) =>
                            {
                                
                                Console.WriteLine("Blood pressure check invoked");

                                if (!storeAPI.CheckForMeasuremntInLastNMinutes("blood_pressure", 6, int.Parse(userIdStr)))
                                {
                                    Console.WriteLine("Blood pressure wasn't measured");

                                    InformCaregivers(userURIPath, "heart", "high", 
                                                     Loc.Get(LANG, Loc.MSG, Loc.MEASUREMENT_IGNORED, Loc.CAREGVR),
                                                     Loc.Get(LANG, Loc.DES, Loc.MEASUREMENT_IGNORED, Loc.CAREGVR));
                                }
                            };

                        }
                        else if(msg.Contains("weight")){

                            Console.WriteLine("ACK Weight measurement!");

                            // set key
                            key = userURIPath + "_weight";

                            var aTimer = new System.Timers.Timer(WAIT_MS);
                            aTimer.AutoReset = false;
                            aTimer.Start();
                            aTimer.Elapsed += (x, y) =>
                            {

                                if (!storeAPI.CheckForMeasuremntInLastNMinutes("weight", 6, int.Parse(userIdStr)))
                                {
                                    Console.WriteLine("Weight wasn't measured");

                                    InformCaregivers(userURIPath, "weight", "high",
                                                     Loc.Get(LANG, Loc.MSG, Loc.MEASUREMENT_IGNORED_WEIGHT, Loc.CAREGVR),
                                                     Loc.Get(LANG, Loc.DES, Loc.MEASUREMENT_IGNORED_WEIGHT, Loc.CAREGVR));
                                }

                            };
                        }

                        Console.WriteLine("[ReminderHandler] userReminder map key: " + key);
                        userReminderMap.Remove(key);

                    }
                    else {
                        Console.WriteLine("Reminder snoozed");

                        // send the BP snoozed notification only if it is relating to a BP reminder journal entry

                        if(msg.Contains("blood") || msg.Contains("pressure")) {
                            // set key
                            key = userURIPath + "_blood_pressure";

                            Console.WriteLine("Snoozed blood pressure measurement!");
                            InformCaregivers(userURIPath, "heart", "medium", Loc.Get(LANG, Loc.MSG, Loc.MEASUREMENT_POSTPONED, Loc.CAREGVR), Loc.Get(LANG, Loc.DES, Loc.MEASUREMENT_POSTPONED, Loc.CAREGVR));
                        }
                        else if(msg.Contains("weight")) {
                            // set key
                            key = userURIPath + "_weight";

                            Console.WriteLine("Snoozed weight measurement!");
                            InformCaregivers(userURIPath, "weight", "medium", Loc.Get(LANG, Loc.MSG, Loc.MEASUREMENT_POSTPONED_WEIGHT, Loc.CAREGVR),
                                Loc.Get(LANG, Loc.DES, Loc.MEASUREMENT_POSTPONED_WEIGHT, Loc.CAREGVR));
                        }

                        Console.WriteLine("[ReminderHandler] userReminder map key: " + key);
                        userReminderMap.Remove(key);
                    }
                }
            }

        }
    }
}

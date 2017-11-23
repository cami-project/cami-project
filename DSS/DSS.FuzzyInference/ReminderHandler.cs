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


            LibratoSettings.Settings.Username = "proiect.cami@gmail.com";
            LibratoSettings.Settings.ApiKey = "14a8816700f5e42443e593720b24eecb8fa3fddc4786dce640ee551556d7e484";
            


            MetricsPublisher.Start();

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

            if(reminder.category.ToString().ToLower() == "user_notifications"){
                
                var key = reminder.content.value.user.id.ToString();
                var userURIPath = "/api/v1/user/" + key + "/";

                var LANG = storeAPI.GetLang(userURIPath);


                if (reminder.content.name == "exercise_started")
                {


                    Console.WriteLine("Exercise started");

                    userActiveExerciseMap.Add(key, reminder);
                    string exerciseType = reminder.content.value.exercise_type.ToString();

                    InformCaregivers(userURIPath, "exercise", "none", Loc.Get(LANG, Loc.MSG, Loc.EXERCISE_STARTED, Loc.CAREGVR), Loc.Get(LANG, Loc.DES, string.Format(Loc.EXERCISE_STARTED, exerciseType), Loc.CAREGVR));
                    return;
                }
                else if (reminder.content.name == "exercise_ended")
                {

                    Console.WriteLine("Exercise ended");

                    if (userActiveExerciseMap.ContainsKey(key) && reminder.content.value.session_uuid == userActiveExerciseMap[key].content.value.session_uuid)
                    {


                        Console.WriteLine("Exercise ended is in the active map");


                        float score = float.Parse(reminder.content.value.score.ToString());
                        string exerciseType = reminder.content.value.exercise_type.ToString();

                        InformCaregivers(userURIPath,
                                         "exercise",
                                         "none",
                                         Loc.Get(LANG, Loc.MSG, Loc.EXERCISE_ENDED, Loc.CAREGVR),
                                         Loc.Get(LANG, Loc.DES, string.Format(Loc.EXERCISE_ENDED, exerciseType, score), Loc.CAREGVR));


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



                if(userReminderMap.ContainsKey(key)){

                    userReminderMap.Remove(key);
                }
                userReminderMap.Add(key, reminder);

                //Reminder issued
                if (reminder.content.name == "reminder_sent")
                {
                    MetricsPublisher.Current.Increment("cami.event.reminder.sent", 1);

                    Console.WriteLine("[reminder_handler] reminder sent entered");
                    var journalId = reminder.content.value.journal.id_enduser.ToString();
                    var journalEntry = storeAPI.GetJournalEntryById(journalId);

                    Console.WriteLine("[reminder_handler] Sent journal entry message for reminder: " + journalEntry.message.ToString());
                    var expectedMessage = Loc.Get(LANG, Loc.MSG, Loc.REMINDER_SENT, Loc.USR);

                    // if it is a reminder_sent event for a BP measurement in the morning
                    if(journalEntry.message.ToString() == expectedMessage) {
                        //Check if reminder is acknowledged after 6 mins
                        var aTimer = new System.Timers.Timer(WAIT_MS);
                        aTimer.Start();
                        aTimer.AutoReset = false;
                        aTimer.Elapsed += (x, y) =>
                        {
                            //This is in case user didn't respond
                            if(userReminderMap[key].content.name == "reminder_sent"){

                                MetricsPublisher.Current.Increment("cami.event.reminder.ignored", 1);
                                Console.WriteLine("Reminder wasn't acknowledged after 6 min");

                                InformCaregivers(userURIPath, "appointment", "high", Loc.Get(LANG, Loc.MSG, Loc.REMINDER_IGNORED, Loc.CAREGVR), Loc.Get(LANG, Loc.DES, Loc.REMINDER_IGNORED, Loc.CAREGVR));
                                userReminderMap.Remove(key);
                            }
                        };
                    }
                }

                //Reminder acknowledged 
                if(reminder.content.name == "reminder_acknowledged") {


                    var ack = reminder.content.value.ack.ToString() == "ok" ? true : false;
                    var journalId = reminder.content.value.journal.id.ToString();
                    var journalEntry = storeAPI.GetJournalEntryById(journalId);

                    Console.WriteLine(journalId);
                    storeAPI.PatchJournalEntry(journalId, ack);

                    if(ack) {
                        MetricsPublisher.Current.Increment("cami.event.reminder.ack", 1);

                        Console.WriteLine("Reminder acknowledged");

                        //Do this just in case it's possible to check if user did something for example 
                        //there is a new value in weight measurements and ignore if it's not possible 
                        //for example medication

                        Console.WriteLine("Type: " + journalEntry.type.ToString());
                        Console.WriteLine("[reminder_handler] Acknowledged journal entry message: " + journalEntry.message.ToString());

                        var expectedMessage = Loc.Get(LANG, Loc.MSG, Loc.REMINDER_SENT, Loc.USR);

                        if(journalEntry.message.ToString() == expectedMessage) {
                            
                            var aTimer = new System.Timers.Timer(WAIT_MS);
                            aTimer.AutoReset = false;
                            aTimer.Start();
                            aTimer.Elapsed += (x, y) =>
                            {

                                Console.WriteLine("Blood pressure");

                                if(!storeAPI.CheckForMeasuremntInLastNMinutes("blood_pressure", 6, int.Parse(key)))
                                {
                                    Console.WriteLine("Blood pressure wasn't measured");
                                    InformCaregivers(userURIPath, "appointment", "high", Loc.Get(LANG, Loc.MSG, Loc.MEASUREMENT_IGNORED, Loc.CAREGVR), Loc.Get(LANG, Loc.DES, Loc.MEASUREMENT_IGNORED, Loc.CAREGVR));
                                }
                            };
                        }

                        userReminderMap.Remove(key);

                    }
                    else {

                        MetricsPublisher.Current.Increment("cami.event.reminder.snoozed", 1);
                        Console.WriteLine("Reminder snoozed");

                        var expectedMessage = Loc.Get(LANG, Loc.MSG, Loc.REMINDER_SENT, Loc.USR);

                        if(journalEntry.message.ToString() == expectedMessage) {
                            // send the BP snoozed notification only if it is relating to a BP reminder journal entry
                            InformCaregivers(userURIPath, "appointment", "high", Loc.Get(LANG, Loc.MSG, Loc.REMINDER_POSTPONED, Loc.CAREGVR), Loc.Get(LANG, Loc.DES, Loc.REMINDER_POSTPONED, Loc.CAREGVR));
                        }

                        userReminderMap.Remove(key);
                    }
                }
            }
        }
    }
}

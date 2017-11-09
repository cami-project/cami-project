﻿using System;
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


       // private const int WAIT_MS = 60 * 1000;

        private const int WAIT_MS = 6 * 60 * 1000;

        public ReminderHandler()
        {
            insertionAPI = new RMQ.INS.InsertionAPI("http://cami-insertion:8010/api/v1/insertion");
            storeAPI = new StoreAPI("http://cami-store:8008");
            //storeAPI = new StoreAPI("http://141.85.241.224:8008");
            //insertionAPI = new RMQ.INS.InsertionAPI("http://cami-insertion:8010/api/v1/insertion");


            userReminderMap = new Dictionary<string, dynamic>();


            LibratoSettings.Settings.Username = "proiect.cami@gmail.com";
            LibratoSettings.Settings.ApiKey = "14a8816700f5e42443e593720b24eecb8fa3fddc4786dce640ee551556d7e484";
            


            MetricsPublisher.Start();

        }
        private void InformCaregivers(string enduserURI, string type, string severity, string msg, string desc)
        {
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

        public void Handle(string json)
        {


            Console.WriteLine("Reminder invoked...");

            var reminder = JsonConvert.DeserializeObject<dynamic>(json);

            if(reminder.category.ToString().ToLower() == "user_notifications"){
                
                var key = reminder.content.value.user.id.ToString();
                var userURIPath = "/api/v1/user/" + key + "/";

                var LANG = storeAPI.GetLang(userURIPath);

                if(userReminderMap.ContainsKey(key)){

                    userReminderMap.Remove(key);
                }
                userReminderMap.Add(key, reminder);

                //Reminder issued
                if (reminder.content.name == "reminder_sent")
                {
                    MetricsPublisher.Current.Increment("cami.event.reminder.sent", 1);

                    Console.WriteLine("reminder sent entered");


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

                            InformCaregivers(key, "reminder", "high", Loc.Get(LANG, Loc.MSG, Loc.REMINDER_IGNORED, Loc.CAREGVR), Loc.Get(LANG, Loc.DES, Loc.REMINDER_IGNORED, Loc.CAREGVR));
                            userReminderMap.Remove(key);
                        }
                    };
                }

                //Reminder acknowledged 
                if(reminder.content.name == "reminder_acknowledged") {


                    var ack = reminder.content.value.ack.ToString() == "ok" ? true : false;
                    var journalId = reminder.content.value.journal.id.ToString();

                    Console.WriteLine(journalId);
                    storeAPI.PatchJournalEntry(journalId, ack);

                    if(ack) {
                        MetricsPublisher.Current.Increment("cami.event.reminder.ack", 1);

                        Console.WriteLine("Reminder acknowledged");

                        var journalEntry = storeAPI.GetJournalEntryById(journalId);

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

                                if(!storeAPI.CheckForMeasuremntInLastNMinutes(journalEntry.type, 6, int.Parse(key)))
                                {
                                    Console.WriteLine("Blood pressure wasn't measured");
                                    InformCaregivers(key, "reminder", "high", Loc.Get(LANG, Loc.MSG, Loc.MEASUREMENT_IGNORED, Loc.CAREGVR), Loc.Get(LANG, Loc.DES, Loc.MEASUREMENT_IGNORED, Loc.CAREGVR));
                                }
                            };
                        }

                        userReminderMap.Remove(key);

                    }
                    else {

                        MetricsPublisher.Current.Increment("cami.event.reminder.snoozed", 1);

                        Console.WriteLine("Reminder snoozed");
                        InformCaregivers(key, "reminder", "high", Loc.Get(LANG, Loc.MSG, Loc.REMINDER_POSTPONED, Loc.CAREGVR), Loc.Get(LANG, Loc.DES, Loc.REMINDER_POSTPONED, Loc.CAREGVR));

                        userReminderMap.Remove(key);

                    }
                }
            }
        }
    }
}

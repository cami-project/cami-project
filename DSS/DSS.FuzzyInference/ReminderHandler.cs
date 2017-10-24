using System;
using System.Collections.Generic;
using DSS.Delegate;
using System.Linq;
using DSS.RMQ;
using Newtonsoft.Json;

namespace DSS.FuzzyInference
{
    public class ReminderHandler : IRouterHandler
    {
        private RMQ.INS.InsertionAPI insertionAPI;
        private StoreAPI storeAPI;
        private Dictionary<string, dynamic> userReminderMap;
        private Dictionary<string, List<int>> userCareGiversMap;

        public ReminderHandler()
        {
            insertionAPI = new RMQ.INS.InsertionAPI("http://cami-insertion:8010/api/v1/insertion");
            storeAPI = new StoreAPI("http://cami-store:8008");
            userReminderMap = new Dictionary<string, dynamic>();
        }



        public void Handle(string json)
        {
            Console.WriteLine("Reminder invoked...");

            var reminder = JsonConvert.DeserializeObject<dynamic>(json);

            if(reminder.category.ToLower() == "user_notifications"){
                
                var key = reminder.content.value.user.id;

                if(userReminderMap.ContainsKey(key)){

                    userReminderMap.Remove(key);
                }
                userReminderMap.Add(key, reminder);

                //Reminder issued
                if (reminder.content.name == "reminder_sent")
                {
                    //Check if reminder is acknowledged after 6 mins
                    var aTimer = new System.Timers.Timer(6 * 60 * 1000);
                    aTimer.Elapsed += (x, y) =>
                    {
                        //This is in case user didn't respond 
                        if(userReminderMap[key].content.name == "reminder_sent"){
                            
                            userCareGiversMap.Add(key, Enumerable.ToList(userCareGiversMap[key].content.value.journal.id_caregivers));

                            foreach (int item in userCareGiversMap[key])
                            {
                                insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = "Jim didn't respond to the reminder!", user_id = item }));
                            }

                            userReminderMap.Remove(key);
                            userCareGiversMap.Remove(key);
                        }
                    };
                }

                //Reminder acknowledged 
                if(reminder.content.name == "reminder_acknowledged") {
                    
                    var ack = reminder.content.value == "ok" ? true : false;
                    var journalId = reminder.content.value.journal.id;
                    storeAPI.PatchJournalEntry(reminder.content.value.journal.id, ack);

                    //Schedule a check in mesurements in 6 min
                    if(ack) {

                        var journalEntry = storeAPI.GetJournalEntryById(journalId);

                        //Do this just in case it's possible to check if user did something for example 
                        //there is a new value in weight measurements and ignore if it's not possible 
                        //for example medication

                        if(journalEntry.type == "weight"){
                            
                            var aTimer = new System.Timers.Timer(6 * 60 * 1000);
                            aTimer.Elapsed += (x, y) =>
                            {
                                if(storeAPI.CheckForMeasuremntInLastNMinutes(journalEntry.type, 6, int.Parse(key)))
                                {

                                    foreach (int item in userCareGiversMap[key])
                                    {
                                        insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = "Jim didn't respond to the reminder!", user_id = item }));
                                    }

                                    userReminderMap.Remove(key);
                                    userCareGiversMap.Remove(key);
                                }
                            };

                        }


                    }
                    //generate 2nd reminder 
                    else {
                        
                        foreach (int item in Enumerable.ToList(reminder.content.value.journal.id_caregivers))
                        {
                            insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = "Jim postponed the reminder!", user_id = item }));

                        }
                        
                    }
                }
            }
        }
    }





}


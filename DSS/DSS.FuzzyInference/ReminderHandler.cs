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

                if(userReminderMap.ContainsKey(reminder.content.name)){

                    userReminderMap.Remove(key);
                }
                userReminderMap.Add(key, reminder);


                //Reminder issued
                if (reminder.content.name == "reminder_sent")
                {
                    //grenderate 2nd reminder after 6 min but first check if it's acknowledged 

                    var aTimer = new System.Timers.Timer(6 * 60 * 1000);
                    aTimer.Elapsed += (x, y) =>
                    {

                        //This is in case user didn't respond 
                        if(key == "reminder_sent"){
                            
                            userCareGiversMap.Add(key, Enumerable.ToList(reminder.content.value.journal.id_caregivers));

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
                    storeAPI.PatchJournalEntry(reminder.content.value.journal.id, ack);

                    //Schedule a check in mesurements in 6 min
                    if(ack) {

                        var aTimer = new System.Timers.Timer(6 * 60 * 1000);
                        aTimer.Elapsed += (x, y) =>
                        {

                            //Check measuremnts for the last 6 mins
                            //if (key == "reminder_sent")
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


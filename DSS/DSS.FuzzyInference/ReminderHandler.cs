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
        private Dictionary<string, int []> userCareGiversMap;


        private const int WAIT_MS = 60 * 1000;

        //private const int WAIT_MS = 6 * 60 * 1000;

        public ReminderHandler()
        {
            insertionAPI = new RMQ.INS.InsertionAPI("http://cami-insertion:8010/api/v1/insertion");
            storeAPI = new StoreAPI("http://cami-store:8008");
            //storeAPI = new StoreAPI("http://141.85.241.224:8008");
            //insertionAPI = new RMQ.INS.InsertionAPI("http://cami-insertion:8010/api/v1/insertion");


            userReminderMap = new Dictionary<string, dynamic>();
            userCareGiversMap = new Dictionary<string, int[]>();
        }



        public void Handle(string json)
        {
            Console.WriteLine("Reminder invoked...");

            var reminder = JsonConvert.DeserializeObject<dynamic>(json);

            if(reminder.category.ToString().ToLower() == "user_notifications"){
                
                var key = reminder.content.value.user.id.ToString();

                if(userReminderMap.ContainsKey(key)){

                    userReminderMap.Remove(key);
                }
                userReminderMap.Add(key, reminder);

                //Reminder issued
                if (reminder.content.name == "reminder_sent")
                {
                    Console.WriteLine("reminder sent entered");

                    userCareGiversMap.Add(key, reminder.content.value.journal.id_caregivers.ToObject<int[]>());

                    //Check if reminder is acknowledged after 6 mins
                    var aTimer = new System.Timers.Timer(WAIT_MS);
                    aTimer.Start();
                    aTimer.AutoReset = false;
                    aTimer.Elapsed += (x, y) =>
                    {
                        //This is in case user didn't respond 
                        if(userReminderMap[key].content.name == "reminder_sent"){



                            Console.WriteLine("Reminder wasn't acknowledged after 6 min");

     
                            foreach (int item in userCareGiversMap[key])
                            {
                                insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = "Jim didn't respond to the reminder!", user_id = item }));
                                Console.WriteLine("sending notification for " + item);
                            }

                            userReminderMap.Remove(key);
                            userCareGiversMap.Remove(key);
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

                        Console.WriteLine("Reminder acknowledged");

                        var journalEntry = storeAPI.GetJournalEntryById(journalId);

                        //Do this just in case it's possible to check if user did something for example 
                        //there is a new value in weight measurements and ignore if it's not possible 
                        //for example medication


                        Console.WriteLine("Type: " + journalEntry.type.ToString());

                        if(journalEntry.type.ToString() == "blood_pressure"){
                            
                            var aTimer = new System.Timers.Timer(WAIT_MS);
                            aTimer.AutoReset = false;
                            aTimer.Start();
                            aTimer.Elapsed += (x, y) =>
                            {

                                Console.WriteLine("Blood pressure");

                                if(!storeAPI.CheckForMeasuremntInLastNMinutes(journalEntry.type, 6, int.Parse(key)))
                                {
                                    Console.WriteLine("Blood pressure (fase)");

                                    foreach (int item in userCareGiversMap[key])
                                    {
                                        insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = "Jim didn't respond to the reminder!", user_id = item }));
                                    }
                                }else{
                                    Console.WriteLine("Blood pressure (true)");


                                }
                            };
                        }

                        userReminderMap.Remove(key);
                        userCareGiversMap.Remove(key);

                    }
                    else {

                        Console.WriteLine("Reminder snoozed");

                        foreach (int item in userCareGiversMap[key])
                        {
                            insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = "Jim postponed the reminder!", user_id = item }));

                        }

                        userReminderMap.Remove(key);
                        userCareGiversMap.Remove(key);
                        
                    }
                }
            }
        }
    }
}

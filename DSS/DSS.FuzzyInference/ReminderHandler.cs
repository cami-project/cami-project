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

        public ReminderHandler()
        {
            insertionAPI = new RMQ.INS.InsertionAPI("http://cami-insertion:8010/api/v1/insertion");
            storeAPI = new StoreAPI("http://cami-store:8008");
        }


        private void BackgroundRefresh()
        {

            //TODO: make a request to cami store and pull from journal 
        }


        public void Handle(string json)
        {
            Console.WriteLine("Reminder invoked...");

            var reminder = JsonConvert.DeserializeObject<dynamic>(json);

            //Reminder acknowledged 
            if(reminder.category == "user_notifications"){

                if(reminder.content.name == "reminder_acknowledged") {
                    
                    var ack = reminder.content.value == "ok" ? true : false;
                    storeAPI.PatchJournalEntry(reminder.content.value.journal.id, ack);

                }


            }



        }
    }





}


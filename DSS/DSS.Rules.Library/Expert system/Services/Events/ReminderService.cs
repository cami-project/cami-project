using System;
namespace DSS.Rules.Library
{
    public class ReminderService
    {
        readonly IInform inform;

        //TODO: remeber that there can be multiple reminders for the same user but different name/category
        public ReminderService(IInform inform)
        {
            this.inform = inform;
        }

        public void Register(Event reminder){ 
            
            InMemoryDB.Push(getKey(reminder), reminder);
        }

        public void Snoozed(Event reminder)
        {
            var USR = reminder.getUserURI();
            var LANG = inform.StoreAPI.GetLang(USR);

            Console.WriteLine("Reminder snoozed");
            inform.Caregivers(USR, "appointment", "high", Loc.Get(LANG, Loc.MSG, Loc.REMINDER_POSTPONED, Loc.CAREGVR), Loc.Get(LANG, Loc.DES, Loc.REMINDER_POSTPONED, Loc.CAREGVR));
        
            InMemoryDB.Remove(getKey(reminder));
        }

        public void CheckIfAcknowledged(Event reminder){

            var key = getKey(reminder);

            Console.WriteLine("ACK CALLED BY SHEDULER!");

            if(InMemoryDB.Exists(key)){



                Console.WriteLine("Reminder exists");
                Event e = InMemoryDB.Get<Event>(key);

                if(e.content.name == "reminder_sent"){


                    var LANG = inform.StoreAPI.GetLang(reminder.getUserURI());


                    inform.Caregivers(reminder.getUserURI(), "reminder", "high", 
                                      Loc.Msg(Loc.REMINDER_IGNORED, LANG, Loc.CAREGVR),
                                      Loc.Des(Loc.REMINDER_IGNORED, LANG, Loc.CAREGVR));

                    InMemoryDB.Remove(key);
                }
            }
        }


        public void Acknowledge(Event reminder){
            
            Console.WriteLine("ACK called!");

            var ack = reminder.content.val.ack.ToString() == "ok" ? true : false;
            var journalId = reminder.content.val.journal.id.ToString();
            Console.WriteLine(journalId);
            inform.StoreAPI.PatchJournalEntry(journalId, ack);

            InMemoryDB.Remove(getKey(reminder));

        }

        private string getKey(Event reminder) {

            return reminder.getUserURI() + "-reminder";
        }
        public void CheckAckInDB(Event reminder) {
            var journalId = reminder.content.val.journal.id.ToString();
            var journalEntry = inform.StoreAPI.GetJournalEntryById(journalId);


            var USR = reminder.getUserURI();
            var LANG = inform.StoreAPI.GetLang(USR);

            //Do this just in case it's possible to check if user did something for example 
            //there is a new value in weight measurements and ignore if it's not possible 
            //for example medication

            Console.WriteLine("Type: " + journalEntry.type.ToString());
            Console.WriteLine("[reminder_handler] Acknowledged journal entry message: " + journalEntry.message.ToString());

            var expectedMessage = Loc.Get(LANG, Loc.MSG, Loc.REMINDER_SENT, Loc.USR);

            if (journalEntry.message.ToString() == expectedMessage)
            {
                    Console.WriteLine("Blood pressure");

                    if (!inform.StoreAPI.CheckForMeasuremntInLastNMinutes("blood_pressure", 6, USR ))
                    {
                        Console.WriteLine("Blood pressure wasn't measured");
                        inform.Caregivers(USR, "appointment", "high", Loc.Get(LANG, Loc.MSG, Loc.MEASUREMENT_IGNORED, Loc.CAREGVR), Loc.Get(LANG, Loc.DES, Loc.MEASUREMENT_IGNORED, Loc.CAREGVR));
                    }
            }
        }

        public void SendWeightReminder(string id)
        {

            Console.WriteLine("Gateway: " + id);
            string uri = "";

            try
            {
                uri = inform.StoreAPI.GetUserOfGateway(id);
                Console.WriteLine("Reminder for weight sent from dss: " + uri);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Filed while trying to get user for the gateway" + id);
                Console.WriteLine(ex);
            }

            InMemoryDB.AddReminder(id, new InternalEvent("SENT", "WEIGHT"));

            try
            {
                inform.User(uri, "reminder", "high", Loc.Msg(Loc.REMINDER_SENT_WEIGHT, Loc.EN, Loc.USR), Loc.Des(Loc.REMINDER_SENT_WEIGHT, Loc.EN, Loc.USR));

            }
            catch (Exception ex)
            {
                Console.WriteLine("Filed while trying to inform user" + uri);
                Console.WriteLine(ex);
            }
        }

        public void SendMorningBloodPressureReminder(string id)
        {
            var uri = inform.StoreAPI.GetUserOfGateway(id);
            Console.WriteLine("Reminder for morning blood pressure  sent from dss: " + uri);

            InMemoryDB.AddReminder(id, new InternalEvent("SENT", "BP_MORNING"));
            inform.User(uri, "reminder", "high", Loc.Msg(Loc.REMINDER_SENT_BP_MORNING, Loc.EN, Loc.USR), Loc.Des(Loc.REMINDER_SENT_BP_MORNING, Loc.EN, Loc.USR));
        }


        public void SendNightBloodPressureReminder(string id)
        {
            var uri = inform.StoreAPI.GetUserOfGateway(id);
            Console.WriteLine("Reminder for night blood pressure  sent from dss: " + uri);

            InMemoryDB.AddReminder(id, new InternalEvent("SENT", "BP_NIGHt"));
            inform.User(uri, "reminder", "high", Loc.Msg(Loc.REMINDER_SENT_BP_MORNING, Loc.EN, Loc.USR), Loc.Des(Loc.REMINDER_SENT_BP_MORNING, Loc.EN, Loc.USR));
        }


    }
}

using System;
namespace DSS.Rules.Library
{
    public class ReminderService
    {
        readonly Inform inform;

        //TODO: remeber that there can be multiple reminders for the same user but different name/category
        public ReminderService(Inform inform)
        {
            this.inform = inform;
        }

        public void Register(Event reminder){
            
            InMemoryDB.Push(getKey(reminder), reminder);
        }

        public void Snoozed(Event reminder)
        {
            var USR = reminder.getUserURI();
            var LANG = inform.storeAPI.GetLang(USR);

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


                    var LANG = inform.storeAPI.GetLang(reminder.getUserURI());


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
            inform.storeAPI.PatchJournalEntry(journalId, ack);

            InMemoryDB.Remove(getKey(reminder));

        }

        private string getKey(Event reminder) {

            return reminder.getUserURI() + "-reminder";
        }
        public void CheckAckInDB(Event reminder) {
            var journalId = reminder.content.val.journal.id.ToString();
            var journalEntry = inform.storeAPI.GetJournalEntryById(journalId);


            var USR = reminder.getUserURI();
            var LANG = inform.storeAPI.GetLang(USR);

            //Do this just in case it's possible to check if user did something for example 
            //there is a new value in weight measurements and ignore if it's not possible 
            //for example medication

            Console.WriteLine("Type: " + journalEntry.type.ToString());
            Console.WriteLine("[reminder_handler] Acknowledged journal entry message: " + journalEntry.message.ToString());

            var expectedMessage = Loc.Get(LANG, Loc.MSG, Loc.REMINDER_SENT, Loc.USR);

            if (journalEntry.message.ToString() == expectedMessage)
            {
                    Console.WriteLine("Blood pressure");

                    if (!inform.storeAPI.CheckForMeasuremntInLastNMinutes("blood_pressure", 6, USR ))
                    {
                        Console.WriteLine("Blood pressure wasn't measured");
                        inform.Caregivers(USR, "appointment", "high", Loc.Get(LANG, Loc.MSG, Loc.MEASUREMENT_IGNORED, Loc.CAREGVR), Loc.Get(LANG, Loc.DES, Loc.MEASUREMENT_IGNORED, Loc.CAREGVR));
                    }
            }

        }



    }
}

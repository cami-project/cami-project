using System;
using System.Collections.Generic;
using DSS.RMQ;
using Newtonsoft.Json;

namespace DSS.Rules.Library
{

    public class State 
    {
        public long TimeStampEnter;
        public long TimeStampMovement;
        public string Name;

        public State(MotionEvent motionEvent)
        {
            Name = motionEvent.getLocationName();
            TimeStampEnter = motionEvent.annotations.timestamp;
        }

    }

    public class  MotionService
    {
        private readonly Inform inform;

        public MotionService(Inform inform)
        {
            this.inform = inform;
        }

        public bool isMorning(Event motion)
        {

            Console.WriteLine("jutro je");
            return TimeService.isMorning(motion);

        }

        private Dictionary<string, State> currentState = new Dictionary<string, State>();

        public void ChangeState(MotionEvent motion) {


            if (currentState.ContainsKey(motion.getGateway())) 
            {
                
                var state = currentState[motion.getGateway()];

                if(state.Name == motion.getLocationName())
                {

                    state.TimeStampMovement = motion.annotations.timestamp;

                    Console.WriteLine("State unchanged: (movement within the same location)");
                }
                else 
                {
                    Console.WriteLine("State changed: " + state.Name + " to " + motion.getLocationName());

                    currentState[motion.getGateway()] = new State(motion);
                }

            }
            else // A fresh state for a new gateway, we assume a geteway is specific for an user
            {

                Console.WriteLine("State added:" + motion.getLocationName());
                currentState.Add(motion.getGateway(), new State(motion));
            }

        }


        public void SendBloodPreasureMeasurementReminder(Event motion)
        {
            Console.WriteLine("Send BP invoked");


            var gatewayURIPath = (string)motion.annotations.source["gateway"];

            var key = "bp-" + gatewayURIPath + DateTime.UtcNow.Date;

            if(!InMemoryDB.Exists(key)) {

                Console.WriteLine("Sending BP notification");

                SendBPMeasurementNotification(inform.storeAPI.GetUserOfGateway(gatewayURIPath));

                InMemoryDB.Push(key, null);
            }
            else {

                Console.WriteLine("BP notification already sent");
            }
        }


        private void SendBPMeasurementNotification(string userURIPath)
        {

            var LANG = inform.storeAPI.GetLang(userURIPath);

            string enduser_msg = Loc.Get(LANG, Loc.MSG, Loc.REMINDER_SENT, Loc.USR);
            string enduser_desc = Loc.Get(LANG, Loc.DES, Loc.REMINDER_SENT, Loc.USR);

            string caregiver_msg = Loc.Get(LANG, Loc.MSG, Loc.REMINDER_SENT, Loc.CAREGVR);
            string caregiver_desc = Loc.Get(LANG, Loc.DES, Loc.REMINDER_SENT, Loc.CAREGVR);

            string notification_type = "appointment";


                inform.User(userURIPath, notification_type, "low", enduser_msg, enduser_msg);
                //TODO: add option of returning the list o IDs
                inform.Caregivers(userURIPath, notification_type, "low", caregiver_msg, caregiver_desc);


                // generate reminder event
                var reminderEvent = new Event()
                {
                    category = "USER_NOTIFICATIONS",
                    content = new Content()
                    {
                        uuid = Guid.NewGuid().ToString(),
                        name = "reminder_sent",
                        value_type = "complex",
                        val = new Dictionary<string, dynamic>()
                        {
                            { "user", new Dictionary<string, int>() { {"id", inform.GetIdFromURI(userURIPath) } } },
                             { "journal", new Dictionary<string, dynamic>() {
                                 { "id_enduser", 2},
                                 { "id_caregivers",new int [3]}
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
            inform.insertionAPI.InsertEvent(JsonConvert.SerializeObject(reminderEvent));
         
        }


        private DateTime UnixTimeStampToDateTime(long unixTimeStamp, TimeZoneInfo tzInfo)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);

            dtDateTime = TimeZoneInfo.ConvertTime(dtDateTime, TimeZoneInfo.Utc, tzInfo);
            return dtDateTime;
        }

        private Tuple<DateTime, DateTime> getMorningLimits(TimeZoneInfo tzInfo)
        {
            DateTime morningStart = DateTime.UtcNow;
            DateTime morningEnd = DateTime.UtcNow;

            morningStart = morningStart.Date.Add(new TimeSpan(6, 0, 0));
            morningEnd = morningEnd.Date.Add(new TimeSpan(11, 0, 0));

            morningStart = TimeZoneInfo.ConvertTime(morningStart, TimeZoneInfo.Utc, tzInfo);
            morningEnd = TimeZoneInfo.ConvertTime(morningEnd, TimeZoneInfo.Utc, tzInfo);

            return new Tuple<DateTime, DateTime>(morningStart, morningEnd);
        }


    }
}

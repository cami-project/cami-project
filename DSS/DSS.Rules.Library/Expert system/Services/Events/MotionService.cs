using System;
using System.Collections.Generic;
using System.Timers;
using DSS.RMQ;
using Newtonsoft.Json;
using System.Linq;

namespace DSS.Rules.Library
{

    public class State 
    {
        public long TimeStampEnter;
        public long TimeStampMovement;

        public string Name;

        public DateTime TimeEnter;
        public DateTime TimeMovement;

        public string Owner;

        public State(MotionEvent motionEvent)
        {

            Owner = motionEvent.getGateway();
            Name = motionEvent.getLocationName();
            TimeStampEnter = motionEvent.annotations.timestamp;

            TimeEnter = TimeService.UnixTimestampToDateTime(TimeStampEnter);
        }

		public override string ToString()
		{
            return this.Name + " " + this.Owner + ": " + TimeEnter.ToShortTimeString() + " - " + TimeMovement.ToShortTimeString();
		}
	}

    public class LocationTimeSpent
    {
        public string Name;
        public int Min;

        public string ID;

        public LocationTimeSpent(string id,string name, int min)
        {
            this.ID = id;
            this.Name = name;
            this.Min = min;
        }

        public override string ToString()
		{
            return Name + " - " + Min;
		}

        public bool Is(string name, int min)
        {
            return this.Name == name && this.Min == min;
        }
	}

    public class LocationChange
    {
        public string Current;
        public string Previous;

        public string ID;


        public LocationChange(string userID, string previous, string current)
        {
            this.ID = userID;
            this.Current = current;
            this.Previous = previous;
        }

        public bool FromTo(string from, string to)
        {
            return from == this.Previous && to == this.Current;
        }


		public override string ToString()
		{
            return "From " + this.Previous + " to " + this.Current;
		}
	}

    public class  MotionService
    {
        private readonly Inform inform;
        private Action<LocationTimeSpent> handleLocationTimeSpent;
        private Action<LocationChange> handleLocationChange;

        public MotionService(Inform inform, Action<LocationTimeSpent> locationTimeSpentHandler, Action<LocationChange> locationChange)
        {
            this.inform = inform;
            this.handleLocationTimeSpent = locationTimeSpentHandler;
            this.handleLocationChange = locationChange;

            Timer timer = new Timer
            {
                Interval = 30 * 1000
            };
            timer.Elapsed += (x, y) => { checkCurrentStateTime(); };
            timer.AutoReset = true;
            timer.Start();
        }

        public bool isMorning(Event motion)
        {
            Console.WriteLine("jutro je");
            return TimeService.isMorning(motion);
        }

        private void checkCurrentStateTime() 
        {
            Console.WriteLine("Current state time");

            var now = DateTime.Now;


            foreach (var item in currentState.Values)
            {
                Console.WriteLine("In " + item.Name + " for " + (now - item.TimeEnter).Minutes+ " min");

                this.handleLocationTimeSpent(new LocationTimeSpent(item.Owner, item.Name, (int)(now - item.TimeEnter).TotalMinutes));
            }
        }

        private Dictionary<string, State> currentState = new Dictionary<string, State>();

        public void ChangeState(MotionEvent motion) 
        {
            
            if (currentState.ContainsKey(motion.getGateway())) 
            {
                Console.WriteLine("CHANGE: ");
                Console.WriteLine(currentState[motion.getGateway()]);

                var state = currentState[motion.getGateway()];

                if(state.Name == motion.getLocationName())
                {
                    state.TimeStampMovement = motion.annotations.timestamp;
                    state.TimeMovement = TimeService.UnixTimestampToDateTime(state.TimeStampMovement);
                    Console.WriteLine("State unchanged: (movement within the same location)");
                }
                else 
                {
                    var id = motion.getGateway();
                    Console.WriteLine("ID: " + id);

                    currentState[id] = new State(motion);

                    InMemoryDB.AddHistory(id, currentState[id]);
                   
                    handleLocationChange(new LocationChange(id, state.Name, motion.getLocationName()));
                    Console.WriteLine("State changed: " + state.Name + " to " + motion.getLocationName());
                }

            }
            else // A fresh state for a new gateway, we assume the geteway is specific for an user
            {

                var id = motion.getGateway();
                Console.WriteLine("ID: " +  id);

                handleLocationChange(new LocationChange(id,"NULL", motion.getLocationName()));
                Console.WriteLine("State added:" + motion.getLocationName());
                currentState.Add(id, new State(motion));
                InMemoryDB.AddHistory(id, currentState[id]);
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

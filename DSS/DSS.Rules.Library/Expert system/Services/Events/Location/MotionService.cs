using System;
using System.Collections.Generic;
using System.Timers;
using DSS.RMQ;
using Newtonsoft.Json;
using System.Linq;

namespace DSS.Rules.Library
{

    public class  MotionService
    {
        private readonly IInform inform;
        private Action<LocationTimeSpent> handleLocationTimeSpent;
        private Action<LocationChange> handleLocationChange;
        private Action<Activity> handleActivity;

        public MotionService(IInform inform, Action<LocationTimeSpent> locationTimeSpentHandler, Action<LocationChange> locationChange, Action<Activity> activityHandler)
        {
            this.inform = inform;
            this.handleLocationTimeSpent = locationTimeSpentHandler;
            this.handleLocationChange = locationChange;
			this.handleActivity = activityHandler;

            Timer timer = new Timer
            {
                Interval = 30 * 1000
            };
            timer.Elapsed += (x, y) => { checkCurrentStateTime(); };
            timer.AutoReset = true;
            timer.Start();
        }

        public void MightBeSleeping(SheduledEvent sheduledEvent)
        {
            Console.WriteLine("Might be sleeping invoked!");
            inform.ActivityLog.Log(new Activity(sheduledEvent.Owner, ActivityType.MightBeSleeping, "The user might be sleeping" , "MotionService.MightBeSleeping(sheduledEvent)"));
        }

        public bool isMorning(Event motion)
        {
            Console.WriteLine("jutro je");
            return TimeService.isMorning(motion);
        }

        public bool isSleepingTime(Event motion)
        {
            Console.WriteLine("Is sleeping time invoked in the motion service");
            return TimeService.isSleepingTime(motion);
        }


        private Dictionary<string, LocationState> currentState = new Dictionary<string, LocationState>();

        private void checkCurrentStateTime() 
        {
            Console.WriteLine("Current state time");
            var now = DateTime.Now;

            foreach (var item in currentState.Values)
            {
                Console.WriteLine(item.Owner + " in " + item.Name + " for " + (now - item.TimeEnter).Minutes+ " min");
                this.handleLocationTimeSpent(new LocationTimeSpent(item.Owner, item.Name, (int)(now - item.TimeEnter).TotalMinutes));
            }
        }


        public void ChangeState(Domain.MotionEvent e)
        {
            if(currentState.ContainsKey(e.Owner))
            {
                var current = currentState[e.Owner];

                if(current.Name == e.Location)
                {
                    current.TimeMovement = e.Timestamp;
                    handleLocationChange(new LocationChange(e, e.Location));

                    //handleActivity(new Activity(state.Owner, state.Name, state.TimeMovement));
                    Console.WriteLine("State unchanged: (movement within the same location)");
                }
                else 
                {
                    var previous = current.Name;
                    current = new LocationState(e);

                    //TODO:Check if need a this in memory db
                    InMemoryDB.AddHistory(e.Owner, current);
                   
                    handleLocationChange(new LocationChange(e, previous));
                    //handleActivity(new Activity(id, state.Name, currentState[id].TimeEnter));
                    Console.WriteLine("State changed: " + previous + " to " + current.Name);
                }
            }
            else // A fresh state for a new gateway, we assume the geteway is specific for an user
            {
                
                handleLocationChange(new LocationChange(e, "NULL"));
                Console.WriteLine("New user added: " + e.Owner + " - " + e.Location);

                currentState.Add(e.Owner, new LocationState(e)); 
                InMemoryDB.AddHistory(e.Owner, currentState[e.Owner]);
            }

        }

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

                    handleActivity(new Activity(state.Owner, state.Name, state.TimeMovement));

                    Console.WriteLine("State unchanged: (movement within the same location)");
                }
                else 
                {
                    var id = motion.getGateway();
                    Console.WriteLine("ID: " + id);

                    currentState[id] = new LocationState(motion);

                    InMemoryDB.AddHistory(id, currentState[id]);
                   
                    handleLocationChange(new LocationChange(id, state.Name, motion.getLocationName()));
                    handleActivity(new Activity(id, state.Name, currentState[id].TimeEnter));

                    Console.WriteLine("State changed: " + state.Name + " to " + motion.getLocationName());
                }

            }
            else // A fresh state for a new gateway, we assume the geteway is specific for an user
            {

                var id = motion.getGateway();
                Console.WriteLine("ID: " +  id);

                handleLocationChange(new LocationChange(id,"NULL", motion.getLocationName()));
                Console.WriteLine("State added:" + motion.getLocationName());
                currentState.Add(id, new LocationState(motion));
                InMemoryDB.AddHistory(id, currentState[id]);
            }
        }


        public void SheeduleSleepingCheck(IEvent e, int min)
        {
            Console.WriteLine("Sheduling the sleeping check for " + e.Owner);
            SheduleService.Add(new SheduledEvent(e.Owner, SheduleService.Type.SleepCheck, DateTime.UtcNow.AddMinutes(min)));
            inform.ActivityLog.Log(new Activity(e.Owner, ActivityType.ShedulingSleepingCheck, "A sleep check sheduling activity", "MotionService.Shedule"));
        }

        public void SendBloodPreasureMeasurementReminder(Event motion)
        {
            Console.WriteLine("Send BP invoked");

            var gatewayURIPath = (string)motion.annotations.source["gateway"];

            var key = "bp-" + gatewayURIPath + DateTime.UtcNow.Date;

            if(!InMemoryDB.Exists(key)) {

                Console.WriteLine("Sending BP notification");

                SendBPMeasurementNotification(inform.StoreAPI.GetUserOfGateway(gatewayURIPath));

                InMemoryDB.Push(key, null);
            }
            else {

                Console.WriteLine("BP notification already sent");
            }
        }


        private void SendBPMeasurementNotification(string userURIPath)
        {

            var LANG = inform.StoreAPI.GetLang(userURIPath);

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
            inform.InsertionAPI.InsertEvent(JsonConvert.SerializeObject(reminderEvent));
         
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

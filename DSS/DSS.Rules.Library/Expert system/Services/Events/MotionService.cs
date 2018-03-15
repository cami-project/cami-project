using System;
using System.Collections.Generic;
using DSS.RMQ;
using Newtonsoft.Json;

namespace DSS.Rules.Library
{
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

            Console.WriteLine("is morning BP");


            var gatewayURIPath = (string)motion.annotations.source["gateway"];
            //var deviceURIPath = (string)motion.annotations.source["sensor"];

            // make a call to the store API to get the user from the gateway
            var userURIPath = inform.storeAPI.GetUserOfGateway(gatewayURIPath);


            Console.WriteLine(userURIPath);

            int userID = inform.GetIdFromURI(userURIPath);
            Tuple<string, string> userLocales = inform.storeAPI.GetUserLocale(userURIPath, userID);

            long timestamp = motion.annotations.timestamp;

            // get localized datetime
            TimeZoneInfo localTz = TimeZoneInfo.FindSystemTimeZoneById(userLocales.Item2);
            DateTime dtime = UnixTimeStampToDateTime(timestamp, localTz);
            DateTime currentTime = UnixTimeStampToDateTime((long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds, localTz);

            Tuple<DateTime, DateTime> morningLimits = getMorningLimits(localTz);

            // check if current dtime is within limits
            if (dtime >= morningLimits.Item1 && currentTime >= morningLimits.Item1 && dtime <= morningLimits.Item2 && currentTime <= morningLimits.Item2)
            {
                return true;
            }
            return false;

        }

        public void SendBloodPreasureMeasurementReminder(Event motion)
        {

            Console.WriteLine("Sent BP");

            var gatewayURIPath = (string)motion.annotations.source["gateway"];
            SendBPMeasurementNotification(inform.storeAPI.GetUserOfGateway(gatewayURIPath));

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

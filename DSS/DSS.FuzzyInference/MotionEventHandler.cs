using DSS.Delegate;
using DSS.RMQ;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.FuzzyInference
{
    public class MotionEventHandler : IRouterHandler
    {
        public

        private StoreAPI storeAPI;
        private RMQ.INS.InsertionAPI insertionAPI;

        /**
         * Map that stores the last activation timestamp of the motion sensor for each user uri
         */
        private Dictionary<string, long> lastActivationMap;

        public string Name => "EVENT";
        
        public MotionEventHandler()
        {
            storeAPI = new StoreAPI("http://cami-store:8008");
            insertionAPI = new RMQ.INS.InsertionAPI("http://cami-insertion:8010/api/v1/insertion");

            lastActivationMap = new Dictionary<string, long>();
        }

        public void Handle(string json)
        {
            Console.WriteLine("MOTION event handler invoked ...");
            var eventObj = JsonConvert.DeserializeObject<Event>(json);

            if (eventObj.category.ToLower() == "user_environment")
            {
                // if we are dealing with a presence sensor
                if (eventObj.content.name == "presence")
                {
                    // see if it is a sensor activation
                    if ((bool)eventObj.content.val["alarm_motion"] == true)
                    {
                        // retrieve the gateway and sensor URI from the source annotations
                        var gatewayURIPath = eventObj.annotations.source["gateway"];
                        var deviceURIPath = eventObj.annotations.source["sensor"];

                        // make a call to the store API to get the user from the gateway
                        var userURIPath = storeAPI.getUserOfGateway(gatewayURIPath);

                        if (userURIPath != null)
                        {
                            int userID = GetIdFromURI(userURIPath);
                            Tuple<string, string> userLocales = storeAPI.getUserLocale(userURIPath, userID);

                            if (userLocales != null)
                            {
                                // retrieve timestamp from annotations
                                long timestamp = eventObj.annotations.timestamp;

                                // get localized datetime
                                TimeZoneInfo localTz = TimeZoneInfo.FindSystemTimeZoneById(userLocales.Item2);
                                DateTime dtime = UnixTimeStampToDateTime(timestamp, localTz);
                                
                                // get morning limits
                                Tuple<DateTime, DateTime> morningLimits = getMorningLimits(localTz);

                                // check if current dtime is within limits
                                if (dtime >= morningLimits.Item1 || dtime <= morningLimits.Item2)
                                {
                                    if (lastActivationMap.ContainsKey(userURIPath))
                                    {
                                        long lastTs = lastActivationMap[userURIPath];
                                        DateTime lastDt = UnixTimeStampToDateTime(lastTs, localTz);

                                        // if the lastDt is outside of the morning limits
                                        if (lastDt < morningLimits.Item1)
                                        {
                                            Console.WriteLine("[MotionEventHandler] Identified first activation of motion sensor in morning at : " + dtime.ToString());
                                            // TODO: SEND THE NOTIFICATION FOR BP MEASUREMENTS
                                        }

                                        lastActivationMap[userURIPath] = timestamp;
                                    }
                                    else
                                    {
                                        // if this is the first activation ever within morning limits
                                        Console.WriteLine("[MotionEventHandler] First ever activation of motion sensor in morning at : " + dtime.ToString());
                                        // TODO: SEND THE NOTIFICATION FOR BP MEASUREMENTS

                                        lastActivationMap[userURIPath] = timestamp;
                                    }
                                }
                                else
                                {
                                    // just mark as latest activation
                                    lastActivationMap.Add(userURIPath, timestamp);
                                }

                            }
                            else
                            {
                                Console.WriteLine("[MotionEventHandler] Insufficient locales information in enduser profile endpoint for user: " + userURIPath);
                            }
                        }
                        else
                        {
                            Console.WriteLine("[MotionEventHandler] Skipping motion event handling since no user can be retrieved for gateway: " + gatewayURIPath);
                        }
                    }
                }
            }
        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp, TimeZoneInfo tzInfo)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);

            dtDateTime = TimeZoneInfo.ConvertTime(dtDateTime, TimeZoneInfo.Local, tzInfo);
            return dtDateTime;
        }

        private int GetIdFromURI(string uri)
        {
            string idStr = uri.TrimEnd('/').Split('/').Last();

            int id = Int32.Parse(idStr);
            return id;
        }

        private Tuple<DateTime, DateTime> getMorningLimits(TimeZoneInfo tzInfo)
        {
            DateTime morningStart = DateTime.UtcNow;
            DateTime morningEnd = DateTime.UtcNow;

            morningStart = morningStart.Date.Add(new TimeSpan(6, 0, 0));
            morningEnd = morningEnd.Date.Add(new TimeSpan(11, 0, 0));

            morningStart = TimeZoneInfo.ConvertTime(morningStart, TimeZoneInfo.Local, tzInfo);
            morningEnd = TimeZoneInfo.ConvertTime(morningEnd, TimeZoneInfo.Local, tzInfo);

            return new Tuple<DateTime, DateTime>(morningStart, morningEnd);
        }
        
    }
}

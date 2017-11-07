using System;
using DSS.Delegate;
using DSS.RMQ;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;
using System.Timers;


namespace DSS.FuzzyInference
{

    public class ValueInfoBase {
        public ValueInfoBase () { }
    }

    public class DefaultValueInfo : ValueInfoBase
    {
		[JsonProperty("value")]
        public int Value { get; set; }
    }

    public class PulseValueInfo : ValueInfoBase 
    {
		[JsonProperty("value")]
        public int Value { get; set; }
    }

    public class StepsValueInfo : ValueInfoBase
    {
        [JsonProperty("value")]
        public int Value { get; set; }

        public int start_timestamp { get; set; }

        public int end_timestamp { get; set; }
    }

    public class WeightValueInfo : ValueInfoBase
    {
        [JsonProperty("value")]
        public float Value { get; set; }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private List<string> AlternateWeightPropertyNames = new List<string> { "weight"};

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            // alternate property names for pulse are added to additionalData
            // we check if there are any, set the original pulse variable and then clear out the dictionary;
            foreach (string altPropName in AlternateWeightPropertyNames)
            {
                if (_additionalData.ContainsKey(altPropName))
                {
                    Value = (float)_additionalData[altPropName];
                    break;
                }
            }

            // clear out _additionalData
            _additionalData.Clear();
        }

        public WeightValueInfo()
        {
            _additionalData = new Dictionary<string, JToken>();
        }
    }
    
    public class BloodPressureValueInfo : ValueInfoBase
    {
        public int pulse { get; set; }

        public int diastolic { get; set; }

        public int systolic { get; set; }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        private List<string> AlternatePulsePropertyNames = new List<string> { "pulserate", "heartrate" };

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            // alternate property names for pulse are added to additionalData
            // we check if there are any, set the original pulse variable and then clear out the dictionary;
            foreach (string altPropName in AlternatePulsePropertyNames)
            {
                if (_additionalData.ContainsKey(altPropName)) {
                    pulse = (int)_additionalData[altPropName];
                    break;
                }
            }

            // clear out _additionalData
            _additionalData.Clear();
        }
    

        public BloodPressureValueInfo()
        {
            _additionalData = new Dictionary<string, JToken>();
        }

    }

    public class Measurement
    {
        public string measurement_type { get; set; }
        public string unit_type { get; set; }
        public int timestamp { get; set; }
        public string user { get; set; }
        public string device { get; set; }

        public ValueInfoBase value_info { get; set; }
		public string gateway_id { get; set; }
        public bool ok { get; set; }
        public string id { get; set; }
        public string resource_uri { get; set; }
    }


    public class MeasurementHandler : IRouterHandler
    {
        private StoreAPI storeAPI;
        private RMQ.INS.InsertionAPI insertionAPI;
        private JsonSerializerSettings settings;

        private Dictionary<string, Timer> stepCountAnalysisTimers;

        public MeasurementHandler()
        {
            storeAPI = new StoreAPI("http://cami-store:8008");
			//storeAPI = new StoreAPI("http://141.85.241.224:8008");

            insertionAPI = new RMQ.INS.InsertionAPI("http://cami-insertion:8010/api/v1/insertion");
			//insertionAPI = new RMQ.INS.InsertionAPI("http://141.85.241.224:8010/api/v1/insertion");

            settings = new JsonSerializerSettings();
            settings.Converters.Add(new MeasurementConverter());

            stepCountAnalysisTimers = new Dictionary<string, Timer>();
        }
        //TODO: Make this a util methods since it's inside of a two files
        private void InformCaregivers(string enduserURI, string type, string severity, string msg, string desc ) 
        {
            var caregivers = storeAPI.GetCaregivers(enduserURI);

            foreach (string caregiverURIPath in caregivers)
            {
                int caregiverID = GetIdFromURI(caregiverURIPath);

                storeAPI.PushJournalEntry(caregiverURIPath, type, severity, msg, desc);
                insertionAPI.InsertPushNotification(msg, caregiverID);
            }

        }



        private void InformUser(string enduserURI, string type, string severity, string msg, string desc)
        {

            storeAPI.PushJournalEntry(enduserURI, type,severity, msg, desc);
            insertionAPI.InsertPushNotification(msg, GetIdFromURI(enduserURI));

        }

        public void Handle(string json) 
        {
            Console.WriteLine("Measurement handler invoked");

            var obj = JsonConvert.DeserializeObject<Measurement>(json, settings);
			
            string END_USER_URI = obj.user;
            int userId = storeAPI.GetIdFromURI(obj.user);
            var LANG = storeAPI.GetLang(obj.user);

            if (obj.measurement_type == "weight")
            {
                var weightValInfo = (WeightValueInfo)obj.value_info;

                var val = weightValInfo.Value;
                var kg = storeAPI.GetLatestWeightMeasurement(userId);

                var trend = "normal";

                    if (Math.Abs(val - kg) > 2)
                    {
                        trend = val > kg ? "up" : "down";
                        obj.ok = false;
    				}
    				else 
    				{
    				    obj.ok = true;
    				}

				    storeAPI.PushMeasurement(JsonConvert.SerializeObject(obj));


                if(trend == "down" || trend == "up") {

                    var category = trend == "up" ? Loc.WEIGHT_INC : Loc.WEIGHT_DEC;

                    var endUserMsg = string.Format(Loc.Get(LANG, Loc.MSG, category, Loc.USR), Math.Floor(Math.Abs(val - kg)));
                    var caregiverMsg = string.Format(Loc.Get(LANG, Loc.MSG, category, Loc.CAREGVR), Math.Floor(Math.Abs(val - kg)));

                    InformUser(END_USER_URI, "weight", "medium", endUserMsg, Loc.Get(LANG, Loc.DES, category, Loc.USR));
                    InformCaregivers(END_USER_URI, "weight", "medium", caregiverMsg, Loc.Get(LANG, Loc.DES, category, Loc.CAREGVR));

                }

            }
            else if(obj.measurement_type == "pulse") 
            {

                var pulseValInfo = (PulseValueInfo)obj.value_info;
                var val = pulseValInfo.Value;

                var min = 50;
                var midLow = 60;
                var midHigh = 100;
                var max = 120;

                if (val < min || val > max)
                    obj.ok = false;
                else
                    obj.ok = true;
                

                storeAPI.PushMeasurement(JsonConvert.SerializeObject(obj));


                DateTime pulseDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(obj.timestamp);

                if( pulseDateTime.AddHours(1) > DateTime.UtcNow && pulseDateTime.Hour > 6) {

                    AnalyzePulseValue(val, min, midLow, midHigh, max, END_USER_URI);
                }

                /*
                if (val < min || val > max) 
                {
					obj.ok = false;

                    
					// For the AAL Forum we will analyse each pulse measurement individually because we do not expect them to arrive very often
                    // In any case, we will need a mechanism to switch between an individual and an aggregated analysis, based on context 
                    // (e.g. how frequently do the measurements arrive and what is their source)

                    if(storeAPI.AreLastNHeartRateCritical(3, min, max)) 
                    {
                        var anEvent = new RMQ.INS.Event() { category = "HEART_RATE", content = new RMQ.INS.Content() { num_value = val } };
                        insertionAPI.InsertEvent( JsonConvert.SerializeObject(anEvent));
                    }
                    
				    //storeAPI.PushJournalEntry("Pulse is abnormal", "Pulse is abnormal", "pulse");
				}
                else 
                {
                    obj.ok = true;
				}
                */
			}
            else if (obj.measurement_type == "blood_pressure")
            {
                // TODO: for now we are treating the pulse values from blood_pressure measurements (if they contain one) in the same
                // way in which we would handle pulse measurements from the FitBit. This is mostly for demo purposes and will change in the future.

                // First, just save the blood_pressure measurement in the CAMI Store, marking it as ok, because we perform no analysis on BP values
                obj.ok = true;
                storeAPI.PushMeasurement(JsonConvert.SerializeObject(obj));

                // Then perform analysis on Pulse value, if there is any

                var bpValInfo = (BloodPressureValueInfo)obj.value_info;
                var val = bpValInfo.pulse;

                // if it has pulse information
                if (val != 0)
                {
                    var min = 50;
                    var midLow = 60;
                    var midHigh = 100;
                    var max = 120;

                    // create a new measurement object of type Pulse, copying over meta data from the BP obj
                    var pulseObj = new Measurement();
                    pulseObj.measurement_type = "pulse";
                    pulseObj.unit_type = "bpm";

                    pulseObj.timestamp = obj.timestamp;
                    pulseObj.user = obj.user;
                    pulseObj.device = obj.device;
                    pulseObj.gateway_id = obj.gateway_id;

                    var pulseValInfo = new PulseValueInfo();
                    pulseValInfo.Value = val;
                    pulseObj.value_info = pulseValInfo; 

                    if (val < min || val > max) 
                        pulseObj.ok = false;
                    else
                        pulseObj.ok = true;

                    
                    // first store the measurement in the CAMI Store
                    storeAPI.PushMeasurement(JsonConvert.SerializeObject(pulseObj));

                    // TODO: currently we know that notification are handled client side only for the CamiDemo user (id = 2), so if we are not
                    // handling data for that user, do not send alerts

                     AnalyzePulseValue(val, min, midLow, midHigh, max, END_USER_URI);

                }
            }
            else if (obj.measurement_type == "steps")
            {
                obj.ok = true;
                storeAPI.PushMeasurement(JsonConvert.SerializeObject(obj));

                // start the step count timer for this user if not already done so
                StartStepsTimer(obj.user);
            }
            else
            {
                // for measurements for which there is no analysis, just mark the measurement as ok and insert it in the CAMI Store
                obj.ok = true;
                storeAPI.PushMeasurement(JsonConvert.SerializeObject(obj));
            }
		}
        
        /**
         * Starts a daily timer task for the stepCountAnalysis if one does not exist for the given user
        */
        private void StartStepsTimer(string userURIPath)
        {
            //For now, the timer has to be refreshed every day 
            //This will be fixed int he future
            var key = userURIPath + DateTime.UtcNow.ToShortDateString();

            if (!stepCountAnalysisTimers.ContainsKey(key))
            {
                // set the moment to run the timer at 19:00 localized time
                // TODO
                var localHour = 19;
                var localMin = 0;

				// TODO: add the recurring Timer to the dictionary
                var timer = new Timer();
                stepCountAnalysisTimers.Add(key, timer);

                // TODO: create a scheduling timer, whose sole job is that of 
                //  - calling the AnalyzeStepCount the first time
                //  - launch the timer that is responsible for the daily re-calling of AnalyzeStepCount

                StartTimer(ChangeTime(DateTime.UtcNow, localHour, localMin), timer, userURIPath);
            }
        }

        public DateTime ChangeTime(DateTime date, int hour, int min)
        {
            return date.Date + new TimeSpan(hour, min, 0);
        }

        public void StartTimer(DateTime time, Timer timer, string userURI)
        {

            if (time < DateTime.UtcNow)
                return;
            
            var interval = (time - DateTime.UtcNow).TotalMilliseconds;
            //var interval = 600000;
            Console.WriteLine("Interval until the next indication :" + interval);

            timer.Interval = interval;
            timer.Enabled = true;
            timer.AutoReset = false;
            timer.Elapsed += (sender, args) =>
            {
                AnalyzeStepCount(userURI);        
            };
        }

        private int GetIdFromURI(string uri)
        {
            string idStr = uri.TrimEnd('/').Split('/').Last();

            int id = Int32.Parse(idStr);
            return id;
        }


        private void AnalyzeStepCount(string userURIPath)
        {
            var LANG = storeAPI.GetLang(userURIPath);

            var now = DateTime.UtcNow;
            var startTs = (long)ChangeTime(now,0,0).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            var endTs = (long)now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds; ;

            int userStepCount = storeAPI.GetUserStepCount(userURIPath, startTs, endTs);

            Console.WriteLine("Analize steps count " + userStepCount);

            if(userStepCount < 1000){
             
                var caregiverMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.STEPS_LESS_1000, Loc.CAREGVR), userStepCount);
                InformCaregivers(userURIPath, "steps", "high", caregiverMsg, Loc.Get(LANG, Loc.DES, Loc.STEPS_LESS_1000, Loc.CAREGVR));
            }
            else if(userStepCount < 2000 ){

                var endUserMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.STEPS_BETWEEN_1000_2000, Loc.USR), userStepCount);
                InformUser(userURIPath, "steps", "medium", endUserMsg, Loc.Get(LANG, Loc.DES, Loc.STEPS_BETWEEN_1000_2000, Loc.USR) );
            }
            else if(userStepCount > 6000) {
                
                var endUserMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.STEPS_BIGGER_6000, Loc.USR), userStepCount);
                InformUser(userURIPath, "steps", "low", endUserMsg, string.Format(Loc.Get(LANG, Loc.DES, Loc.STEPS_BIGGER_6000, Loc.USR), userStepCount));
            }
                
        }

        private void AnalyzePulseValue(int val, int min, int midLow, int midHigh, int max, string END_USER_URI)
        {

            var LANG = storeAPI.GetLang(END_USER_URI);

            if (val < min)
            {
                var endUserMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.PULSE_LOW, Loc.USR), val);
                var caregiverMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.PULSE_LOW, Loc.CAREGVR), val);

                InformUser(END_USER_URI, "heart", "high", endUserMsg, Loc.Get(LANG, Loc.DES, Loc.PULSE_LOW, Loc.USR));
                InformCaregivers(END_USER_URI, "heart", "high", caregiverMsg, Loc.Get(LANG, Loc.DES, Loc.PULSE_LOW, Loc.CAREGVR));
            }
            else if (val < midLow)
            {
                var endUserMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.PULSE_MID_LOW, Loc.USR), val);
                var caregiverMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.PULSE_MID_LOW, Loc.CAREGVR), val);

                InformUser(END_USER_URI, "heart", "medium", endUserMsg, Loc.Get(LANG, Loc.DES, Loc.PULSE_MID_LOW, Loc.USR));
                InformCaregivers(END_USER_URI, "heart", "medium", caregiverMsg,Loc.Get(LANG, Loc.DES, Loc.PULSE_MID_LOW, Loc.CAREGVR));
            }
            if (val > max)
            {
                var endUserMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.PULSE_MEDIUM, Loc.USR), val);
                var caregiverMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.PULSE_MEDIUM, Loc.CAREGVR), val);

                InformUser(END_USER_URI, "heart", "high", endUserMsg, Loc.Get(LANG, Loc.DES, Loc.PULSE_MEDIUM, Loc.USR));
                InformCaregivers(END_USER_URI, "heart", "high", caregiverMsg, Loc.Get(LANG, Loc.DES, Loc.PULSE_MEDIUM, Loc.CAREGVR));
             }
            else if (val > midHigh)
            {
                var endUserMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.PULSE_HIGH, Loc.USR), val);
                var caregiverMsg = string.Format(Loc.Get(LANG, Loc.MSG, Loc.PULSE_HIGH, Loc.CAREGVR), val);

                InformUser(END_USER_URI, "heart", "medium", endUserMsg, Loc.Get(LANG, Loc.DES, Loc.PULSE_HIGH, Loc.USR));
                InformCaregivers(END_USER_URI, "heart", "medium", caregiverMsg, Loc.Get(LANG, Loc.DES, Loc.PULSE_HIGH, Loc.CAREGVR));
            }
        }
	}
}



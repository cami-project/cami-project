﻿using System;
using DSS.Delegate;
using DSS.RMQ;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;

namespace DSS.FuzzyInference
{

    public class ValueInfoBase {
        public ValueInfoBase () { }
    }

    public class PulseValueInfo : ValueInfoBase 
    {
		[JsonProperty("value")]
        public int Value { get; set; }
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


    //public class BloodPressureValueConverter : JsonConverter
    //{
    //    private List<string> AcceptedPulsePropertyNames = new List<string> { "pulse", "pulserate", "heartrate" };

    //    public override bool CanConvert(Type objectType)
    //    {
    //        return typeof(BloodPressureValueInfo).IsAssignableFrom(objectType);
    //    }

    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        object instance = objectType.GetConstructor(Type.EmptyTypes).Invoke(null);
    //        System.Reflection.PropertyInfo[] props = objectType.GetProperties();

    //        JObject jo = JObject.Load(reader);
    //        foreach (JProperty jp in jo.Properties())
    //        {
    //            if (AcceptedPulsePropertyNames.Contains<string>(jp.Name, StringComparer.OrdinalIgnoreCase))
    //            {
    //                PropertyInfo prop = props.FirstOrDefault(pi => pi.CanWrite && string.Equals(pi.Name, "pulse", StringComparison.OrdinalIgnoreCase));

    //                if (prop != null)
    //                    prop.SetValue(instance, jp.Value.ToObject(prop.PropertyType, serializer));
    //            }
    //        }

    //        return instance;
    //    }

    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        BloodPressureValueInfo bpInfo = (BloodPressureValueInfo)value;

    //        JObject jObj = JObject.FromObject(bpInfo, serializer);
    //        jObj.WriteTo(writer);
    //    }
    //}


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
        public string Name => "MEASUREMENT";
        private JsonSerializerSettings settings;

        public MeasurementHandler()
        {

            storeAPI = new StoreAPI("http://cami-store:8008/api/v1");
			//storeAPI = new StoreAPI("http://141.85.241.224:8008/api/v1");

            insertionAPI = new RMQ.INS.InsertionAPI("http://cami-insertion:8010/api/v1/insertion");
			//insertionAPI = new RMQ.INS.InsertionAPI("http://141.85.241.224:8010/api/v1/insertion");


		    settings = new JsonSerializerSettings();
			settings.Converters.Add(new MeasurementConverter());
		}

        public void Handle(string json) 
        {
            Console.WriteLine("Measurement handler invoked");

            // TODO: This will have to change to dynamic handling of end-user and caregiver URIs, as obtained from the measurement that is actually being handled.
            string END_USER_URI = "/api/v1/user/2/";
            string CAREGIVER_URI = "/api/v1/user/3/";

            var obj = JsonConvert.DeserializeObject<Measurement>(json, settings);
            obj.timestamp = (int) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

			if (obj.measurement_type == "weight")
			{
                
                var weightValInfo = (WeightValueInfo)obj.value_info;

                //var val = float.Parse( obj.value_info.Value);
                var val = weightValInfo.Value;
                var kg = storeAPI.GetLatestWeightMeasurement();

                var trend = "normal";

                if (Math.Abs(val - kg) > 2)                 {
                    trend = val > kg ? "up" : "down";
                    obj.ok = false;
				}                 else                  {                     obj.ok = true;                 }                  // first store measurement in CAMI Store                 storeAPI.PushMeasurement(JsonConvert.SerializeObject(obj));                  // if measurement was not ok generate appropriate alerts                 if (trend == "down")
                {
                    var endUserMsg = string.Format("There's a decrease of {0} kg in your weight.", Math.Abs(val - kg));
                    var endUserDescription = "Please take your meals regularly.";

                    var caregiverMsg = string.Format("Jim lost {0} kg.", Math.Abs(val - kg));
                    var caregiverDescription = "You can contact him and see what's wrong.";

                    // insert journal entry for end-user
                    storeAPI.PushJournalEntry(END_USER_URI, "weight", "medium", endUserMsg, endUserDescription);
                    insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = endUserMsg, user_id = 2 }));

                    // insert journal entry for caregiver
                    storeAPI.PushJournalEntry(CAREGIVER_URI, "weight", "medium", caregiverMsg, caregiverDescription);
                    insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = caregiverMsg, user_id = 3 }));
                }                 else if (trend == "up")
                {
                    var endUserMsg = string.Format("There's an increase of {0} kg in your weight.", Math.Abs(val - kg));
                    var endUserDescription = "Please be careful with your meals.";

                    var caregiverMsg = string.Format("Jim gained {0} kg.", Math.Abs(val - kg));
                    var caregiverDescription = "Please check if this has to do with his diet.";

                    // insert journal entry for end-user
                    storeAPI.PushJournalEntry(END_USER_URI, "weight", "medium", endUserMsg, endUserDescription);
                    insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = endUserMsg, user_id = 2 }));

                    // insert journal entry for caregiver
                    storeAPI.PushJournalEntry(CAREGIVER_URI, "weight", "medium", caregiverMsg, caregiverDescription);
                    insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = caregiverMsg, user_id = 3 }));
                }                 

            }
            else if(obj.measurement_type == "pulse") 
            {

                var pulseValInfo = (PulseValueInfo)obj.value_info;
                //var val = float.Parse(obj.value_info.Value);
                var val = pulseValInfo.Value;

                var min = 50;
                var midLow = 60;
                var midHigh = 100;
                var max = 120;


                if (val < min || val > max)
                    obj.ok = false;
                else
                    obj.ok = true;
                
                // first store the measurement in the CAMI Store
                storeAPI.PushMeasurement(JsonConvert.SerializeObject(obj));

                // then perform detailed analysis to generate alerts if needed
                if (val < min)
                {
                    var endUserMsg = string.Format("Hey Jim! Your heart rate is quite low: {0}.", val);
                    var caregiverMsg = string.Format("Jim's heart rate is dangerously low: only {0}.", val);

                    var endUserDescription = "I have contacted your caregiver.";
                    var caregiverDescription = "Please take action now!";

                    // insert journal entry for end-user
                    storeAPI.PushJournalEntry(END_USER_URI, "heart", "high", endUserMsg, endUserDescription);
                    insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = endUserMsg, user_id = 2 }));

                    // insert journal entry for caregiver
                    storeAPI.PushJournalEntry(CAREGIVER_URI, "heart", "high", caregiverMsg, caregiverDescription);
                    insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = caregiverMsg, user_id = 3 }));
                }
                else if (val < midLow) {
                    var endUserMsg = string.Format("Hey Jim! Your heart rate is just a bit low: {0}.", val);
                    var caregiverMsg = string.Format("Jim's heart rate is a bit low: only {0}.", val);

                    var endUserDescription = "How about some exercise?";
                    var caregiverDescription = "Please make sure he's all right.";

                    // insert journal entry for end-user
                    storeAPI.PushJournalEntry(END_USER_URI, "heart", "medium", endUserMsg, endUserDescription);
                    insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = endUserMsg, user_id = 2 }));

                    // insert journal entry for caregiver
                    storeAPI.PushJournalEntry(CAREGIVER_URI, "heart", "medium", caregiverMsg, caregiverDescription);
                    insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = caregiverMsg, user_id = 3 }));
                }

                if (val > max)
                {
                    var endUserMsg = string.Format("Hey Jim! Your heart rate is quite high: {0}.", val);
                    var caregiverMsg = string.Format("Jim's heart rate is dangerously high: over {0}.", val);

                    var endUserDescription = "I have contacted your caregiver.";
                    var caregiverDescription = "Please take action now!";

                    // insert journal entry for end-user
                    storeAPI.PushJournalEntry(END_USER_URI, "heart", "high", endUserMsg, endUserDescription);
                    insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = endUserMsg, user_id = 2 }));

                    // insert journal entry for caregiver
                    storeAPI.PushJournalEntry(CAREGIVER_URI, "heart", "high", caregiverMsg, caregiverDescription);
                    insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = caregiverMsg, user_id = 3 }));
                }
                else if (val > midHigh)
                {
                    var endUserMsg = string.Format("Hey Jim! Your heart rate is just a bit high: {0}.", val);
                    var caregiverMsg = string.Format("Jim's heart rate is a bit high: over {0}.", val);

                    var endUserDescription = "Why not rest for a bit?";
                    var caregiverDescription = "Please make sure he's alright.";

                    // insert journal entry for end-user
                    storeAPI.PushJournalEntry(END_USER_URI, "heart", "medium", endUserMsg, endUserDescription);
                    insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = endUserMsg, user_id = 2 }));

                    // insert journal entry for caregiver
                    storeAPI.PushJournalEntry(CAREGIVER_URI, "heart", "medium", caregiverMsg, caregiverDescription);
                    insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = caregiverMsg, user_id = 3 }));
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
            else
            {
                // for measurements for which there is no analysis, just mark the measurement as ok and insert it in the CAMI Store
                obj.ok = true;
                storeAPI.PushMeasurement(JsonConvert.SerializeObject(obj));
            }
		}
	}
}



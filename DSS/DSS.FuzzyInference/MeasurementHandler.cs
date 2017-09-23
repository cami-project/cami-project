using System;
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
    }
    public class DefaultValueInfo : ValueInfoBase
	{
		[JsonProperty("value")]
		public float Value { get; set; }
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


            var obj = JsonConvert.DeserializeObject<Measurement>(json, settings);
            obj.timestamp = (int) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

			if (obj.measurement_type == "weight")
			{
                var weightValInfo = (WeightValueInfo)obj.value_info;

                //var val = float.Parse( obj.value_info.Value);
                var val = weightValInfo.Value;
                var kg = storeAPI.GetLatestWeightMeasurement();


               if (Math.Abs(val - kg) > 2)                {                     var msg = val > kg ? "Have lighter meals" : "Have more consistent meals";                     storeAPI.PushJournalEntry(msg, "Abnormal change in weight noticed", "weight");
                    storeAPI.PushJournalEntry("Abnormal change in weight noticed", "Abnormal change in weight noticed", "weight");

					insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = msg, user_id = 2 }));
     
                    obj.ok = false;
				}                 else                  {                     obj.ok = true;                }             }
            else if(obj.measurement_type == "pulse") 
            {
                var pulseValInfo = (PulseValueInfo)obj.value_info;
                //var val = float.Parse(obj.value_info.Value);
                var val = pulseValInfo.Value;

                var min = 50;
                var max = 120;

                if (val < min || val > max) 
                {
					obj.ok = false;

					if(storeAPI.AreLastNHeartRateCritical(3, min, max)) 
                    {
                        var anEvent = new RMQ.INS.Event() { category = "HEART_RATE", content = new RMQ.INS.Content() { num_value = val } };
                        insertionAPI.InsertEvent( JsonConvert.SerializeObject(anEvent));
                    }

				    storeAPI.PushJournalEntry("Pulse is abnormal", "Pulse is abnormal", "pulse");
				}
                else 
                {
                    obj.ok = true;
				}
			}

            storeAPI.PushMeasurement(JsonConvert.SerializeObject(obj));
		}
	}
}



using System;
using System.Collections.Generic;
using DSS.Delegate;
using DSS.RMQ;
using Newtonsoft.Json;

namespace DSS.FuzzyInference
{

    public class Measurement
    {
		public string measurement_type { get; set; }
		public string unit_type { get; set; }
		public int timestamp { get; set; }
		public string user { get; set; }
		public string device { get; set; }
		public string value_info { get; set; }
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

        public MeasurementHandler()
        {
            //API = new RmqAPI("http://cami-store:8008/api/v1");
            storeAPI = new StoreAPI("http://cami-store:8008/api/v1");
            insertionAPI = new RMQ.INS.InsertionAPI("http://cami-insertion:8010/api/v1/insertion");

		}

        public void Handle(string json) 
        {

            var obj = JsonConvert.DeserializeObject<Measurement>(json);

            var result = new List<string>();

			if (obj.measurement_type == "weight")
			{
                
                //TODO: wight data is going to be wrapped inside of an object!
                var val = float.Parse( obj.value_info);

				var kg = storeAPI.GetLatestWeightMeasurement();

               if (Math.Abs(val - kg) > 2)                {                     var msg = val > kg ? "Have lighter meals" : "Have more consistent meals";                     result.Add("Abnormal change in weight noticed - " + msg);                     storeAPI.PushJournalEntry(msg, "Abnormal change in weight noticed");                     insertionAPI.InsertPushNotification(JsonConvert.SerializeObject(new DSS.RMQ.INS.PushNotification() { message = msg, user_id = 2 }));
     
                    obj.ok = false;
				}                 else                  {                     obj.ok = true;                }             }
            else if(obj.measurement_type == "pulse") 
            {
				var val = float.Parse(obj.value_info);
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

					storeAPI.PushJournalEntry("Pulse is abnormal", "Pulse is abnormal");
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



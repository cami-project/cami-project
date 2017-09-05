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
        private StoreAPI API;

        public string Name => "MEASUREMENT";

        public MeasurementHandler()
        {
            //API = new RmqAPI("http://cami-store:8008/api/v1");
            API = new StoreAPI("http://cami.vitaminsoftware.com:8008/api/v1");
        }

        public void Handle(object obje) 
        {

            var obj = obje as Measurement;

            var result = new List<string>();

			if (obj.measurement_type == "weight")
			{
                
                //TODO: wight data is going to be wrapped inside of an object!
                var val = float.Parse( obj.value_info);

				var kg = API.GetLatestWeightMeasurement();

               if (Math.Abs(val - kg) > 2)                {                     var msg = val > kg ? "Have lighter meals" : "Have more consistent meals";                     result.Add("Abnormal change in weight noticed - " + msg);                     API.PushJournalEntry(msg, "Abnormal change in weight noticed");                         					obj.ok = false;                     //and push notification                  }                 else                  {                     obj.ok = true;                     API.PushJournalEntry("Weight is OK", "Weight is OK");                }             }
            else if(obj.measurement_type == "pulse") 
            {
				var val = float.Parse(obj.value_info);
                var min = 50;
                var max = 120;

                if (val < min || val > max) 
                {
					obj.ok = false;

					if(API.AreLastNHeartRateCritical(3, min, max)) 
                    {
                        //generate an event and push it to the event exchange 
                    }


					API.PushJournalEntry("Pulse is abnormal", "Pulse is abnormal");
				}
                else 
                {
                    obj.ok = true;
					API.PushJournalEntry("Pulse is OK", "Pulse is OK");
				}

			}

            API.PushMeasurement(JsonConvert.SerializeObject(obj));
		}
	}
}



using System;
using System.Collections.Generic;
using DSS.Delegate;
using DSS.RMQ;

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
    }


    public class MeasurementHandler : IRouterHandler
    {

        public string Name => "MEASUREMENT";

        public void Handle(object obje) 
        {

            var obj = obje as Measurement;

            var result = new List<string>();

			if (obj.measurement_type == "weight")
			{
                
                //TODO: wight data is going to be wrapped inside of an object!
                var val = float.Parse( obj.value_info);

				var kg = new RmqAPI("").GetLatestWeightMeasurement();

                if (Math.Abs(val - kg) > 2)
				{
                        var msg = val > kg ? "Have lighter meals" : "Have more consistent meals";
                        result.Add("Abnormal change in weight noticed - " + msg);
						new RmqAPI("").PushJournalEntry(msg, "Abnormal change in weight noticed");
				}
			}
		}

    }
}


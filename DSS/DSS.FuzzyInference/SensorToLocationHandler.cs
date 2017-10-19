using System;
using System.Collections.Generic;
using DSS.Delegate;
using DSS.RMQ;
using Newtonsoft.Json;

namespace DSS.FuzzyInference
{
    public class SensorToLocationHandler : IRouterHandler
    {
        private Dictionary<string, string> Map;
		private StoreAPI storeAPI;

		public SensorToLocationHandler()
        { 
            //TODO: This is going to be read from a JSON config file 
            Map = new Dictionary<string, string>();
            Map.Add("/api/v1/device/2/", "Kitchen");
            Map.Add("/api/v1/device/9/", "Living room");

			storeAPI = new StoreAPI("http://cami-store:8008");
		}

        public void Handle(string json)
        {
            string END_USER_URI = "/api/v1/user/2/";


            Console.WriteLine("Sensor handler invoked");

            var deserialized = JsonConvert.DeserializeObject<dynamic>(json);

            if(deserialized.category == "USER_ENVIRONMENT"){

				Console.WriteLine("USER_ENVIRONMENT");


				if(deserialized.content.name == "presence"){
                    
					Console.WriteLine("presence");

					var msg = "User has entered the" + Map[deserialized.annotations.source.sensor];

                    storeAPI.PushJournalEntry(END_USER_URI, "environment", "low", msg, msg);

                    Console.WriteLine("Sensor to location: " + msg);
				}
            }
		}
    }
}

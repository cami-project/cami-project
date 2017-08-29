using System;
using System.Net.Http;

namespace DSS.RMQ
{


	public class ValueInfo
	{
		public int val { get; set; }
	}

	public class Measurement
	{
		public string device { get; set; }
		public int id { get; set; }
		public string measurement_type { get; set; }
		public bool ok { get; set; }
		public int precision { get; set; }
		public string resource_uri { get; set; }
		public int timestamp { get; set; }
		public string unit_type { get; set; }
		public string user { get; set; }
		public ValueInfo value_info { get; set; }
	}


    public class RmqAPI
    {

        public string url { get; set; }

        public RmqAPI(string baseUrl)
        {
            this.url = baseUrl;

		}

        public void PushEvent(string json )
        {

            HttpContent content = new StringContent(json);  
            var response = new HttpClient().PostAsync( url + "/events/", content);

			Console.WriteLine(response.Result);
        }

        public void PushMeasuremnt( string json )
        {
            
            json = @"    {
              ""device"": ""/api/v1/device/2/"",
              ""id"": 105,
              ""measurement_type"": ""pulse"",
              ""ok"": false,
              ""precision"": 100,
              ""resource_uri"": ""/api/v1/measurement/1/"",
              ""timestamp"": 1477413397,
              ""unit_type"": ""bpm"",
              ""user"": ""/api/v1/user/2/"",
              ""value_info"": {}
            }";
     
			HttpContent content = new StringContent(json);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = new HttpClient().PostAsync("http://cami.vitaminsoftware.com:8008/api/v1/measurement/", content);

            Console.WriteLine(response.Result);
        }

        public int GetLatestWeightMeasurement() 
        {

            var response = new HttpClient().GetAsync("http://cami.vitaminsoftware.com:8008/api/v1/measurement?limit=1&order_by=-timestamp");
            Console.WriteLine(response.Result);

            return 0;
        }


        public void PushJournalEntry(string json) 
        {
            json = @"{
              ""user"": ""/api/v1/user/3/"",
              ""severity"": """",
              ""timestamp"": """",
            ""description"": ""Time for your medicine table."",
                  ""id"": 89,
                  ""message"": ""Jim has to take his medicine at 08:00"",
                  ""reference_id"": null,
                  ""resource_uri"": ""/api/v1/journal_entries/89/"",
                  ""severity"": ""none"",
                  ""timestamp"": ""1503905400"",
                  ""type"": ""medication"",
                  ""user"": ""/api/v1/user/3/"", ""type"": """",
              ""reference_id"": null,
              ""description"": """"
            }";
			HttpContent content = new StringContent(json);
			content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = new HttpClient().PostAsync("http://cami.vitaminsoftware.com:8008/api/v1/journal_entries/", content);

			Console.WriteLine(response.Result);

        }




        public void PushNotification(string json) 
        {

            json = @"    {
      ""active"": true,
      ""device_id"": null,
      ""id"": 13,
      ""name"": null,
      ""other_info"": ""{}"",
      ""registration_id"": ""b638da69b963856df03b5e5a3c221161edd55b100dda4b5d66fa2f05e3f3f390"",
      ""resource_uri"": ""/api/v1/pushnotificationdevice/13/"",
      ""type"": ""APNS"",
      ""user"": ""/api/v1/user/3/""
    }";

			HttpContent content = new StringContent(json);
			content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
			//var response = new HttpClient().PostAsync("http://cami.vitaminsoftware.com:8008/api/v1/pushnotificationdevice/", content);
			var response = new HttpClient().PostAsync("http://cami-store:8008/api/v1/pushnotificationdevice/", content);

			Console.WriteLine(response.Result);
            

			
        }
    }
}

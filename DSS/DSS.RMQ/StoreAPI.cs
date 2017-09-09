using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace DSS.RMQ
{


	public class ValueInfo
	{
		public int val { get; set; }
	}

	public class JournalEntry
	{
		public string user { get; set; }
		public string severity { get; set; }
		public string timestamp { get; set; }
		public string description { get; set; }
		public string message { get; set; }
        public string reference_id { get; set; }
		public string resource_uri { get; set; }
		public string type { get; set; }
	}




    public class StoreAPI
    {

        public string url { get; set; }

        public StoreAPI(string baseUrl)
        {
            this.url = baseUrl;
		}

        public void PushEvent(string json )
        {

            HttpContent content = new StringContent(json);  
            var response = new HttpClient().PostAsync( url + "/events/", content);

			Console.WriteLine(response.Result);
        }

        public void PushMeasurement( string json )
        {
			HttpContent content = new StringContent(json);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
           
            var response = new HttpClient().PostAsync( url +"/measurement/", content);

			Console.WriteLine("PUSH MEASUREMNT:" + response.Result);
        }


        public bool AreLastNHeartRateCritical(int n, int low, int high){

            var urlVS = url + "/measurement/?limit=3&measurement_type=pulse&order_by=-timestamp";

            var response = new HttpClient().GetAsync(urlVS);

            if (response.Result.IsSuccessStatusCode)
            {
                dynamic deserialized = JsonConvert.DeserializeObject<dynamic>(response.Result.Content.ReadAsStringAsync().Result);
                var isCritical = true;

                if (n == deserialized["measurements"].Count)
                {
                    foreach (var item in deserialized["measurements"])
                    {
                        if (item["value_info"]["value"] > low && item["value_info"]["value"] < high)
                        {
                            isCritical = false;
						}
                    }
                }
                else 
                {
                    throw new Exception("Server response doesn't match requested number of pulse items " + deserialized);
				}
                return isCritical;
            }
            throw new Exception("Something went wrong on the server side while checking for pulse " + response.Result);
         }

        public float GetLatestWeightMeasurement() 
        {

            var response = new HttpClient().GetAsync(url +"/measurement/?limit=1&measurement_type=weight&order_by=-timestamp");

            if (response.Result.IsSuccessStatusCode)
            {
                dynamic deserialized = JsonConvert.DeserializeObject<dynamic>(response.Result.Content.ReadAsStringAsync().Result);

                return deserialized["measurements"][0]["value_info"]["value"];
            }

			throw new Exception("Something went wrong on the server side while geting last wight measurement " + response.Result);

        }

        public void PushJournalEntry(string msg, string desc) 
        {

            var obj = new JournalEntry()
            {
                user = "/api/v1/user/3/",
                description = desc,
                message = msg,
                severity = "none",
                reference_id = null,
                timestamp = "1503905400",
                type = "medication"
            };

            HttpContent content = new StringContent(JsonConvert.SerializeObject(obj));
			content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
			var response = new HttpClient().PostAsync(url +"/journal_entries/", content);
			//var response = new HttpClient().PostAsync("http://cami-store:8008/api/v1/journal_entries/", content);

			Console.WriteLine("JOURNAL ENTRTY: " + response.Result);

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
            var response = new HttpClient().PostAsync(url+"/pushnotificationdevice/", content);

			Console.WriteLine("PUSH NOTIFICATION" + response.Result);
            

			
        }
    }
}

using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace DSS.RMQ
{


	public class ValueInfo
	{
		public int val { get; set; }
	}

	public class JournalEntry
	{
        [DefaultValue(-1)]
        public int id { get; set; }
        public string user { get; set; }
		public string severity { get; set; }
		public long timestamp { get; set; }
		public string description { get; set; }
		public string message { get; set; }

        [DefaultValue(-1)]
        public long reference_id { get; set; }
		public string resource_uri { get; set; }
		public string type { get; set; }
        public bool acknowledged { get; set; }
	}

    public class StoreAPI
    {

        public string url { get; set; }

        public StoreAPI(string baseUrl)
        {
            this.url = baseUrl;
		}

        public void PushMeasurement( string json )
        {
			HttpContent content = new StringContent(json);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
           
            var response = new HttpClient().PostAsync( url + "/api/v1/measurement/", content);

			//Console.WriteLine("PUSH MEASUREMNT:" + response.Result);
			Console.WriteLine("Measurement inserted");

		}

        public bool AreLastNHeartRateCritical(int n, int low, int high){

            //var urlVS = url + "/measurement/?limit=3&measurement_type=pulse&order_by=-timestamp&user=2&device=2";
            // we want to get all pulse related data for the end-user (user=2), not just the ones from device 2
            var urlVS = url + "/api/v1/measurement/?limit=3&measurement_type=pulse&order_by=-timestamp&user=2";

            var response = new HttpClient().GetAsync(urlVS);

            if (response.Result.IsSuccessStatusCode)
            {
                dynamic deserialized = JsonConvert.DeserializeObject<dynamic>(response.Result.Content.ReadAsStringAsync().Result);
                var isCritical = true;


                //Console.WriteLine(deserialized);

                if (n  == deserialized["measurements"].Count)
                {


                    for (int i = 0; i < deserialized["measurements"].Count; i++)
                    {
                        var item = deserialized["measurements"][i];

                        var hr =  (int) (item["value_info"]["value"] ?? item["value_info"]["Value"]);

                        Console.WriteLine("HR: " + hr);

                        if ( hr > low && hr < high)
                        {
                            isCritical = false;
						}
                    }

					Console.WriteLine("IS critical: " + isCritical);
				}

                else 
                {
                    throw new Exception("Server response doesn't match requested number of pulse items " + deserialized);
				}
                return isCritical;
            }
            throw new Exception("Something went wrong on the server side while checking for pulse " + response.Result);
         }

        public float GetLatestWeightMeasurement(int userId) 
        {

            //var response = new HttpClient().GetAsync(url +"/measurement/?limit=1&measurement_type=weight&order_by=-timestamp&user=2");

            string queryPath = String.Format("/api/v1/measurement/?limit=1&measurement_type=weight&order_by=-timestamp&user={0}", userId);
            var response = new HttpClient().GetAsync(url + queryPath);

            if (response.Result.IsSuccessStatusCode)
            {
                dynamic deserialized = JsonConvert.DeserializeObject<dynamic>(response.Result.Content.ReadAsStringAsync().Result);

                return deserialized["measurements"][0]["value_info"]["value"] ?? deserialized["measurements"][0]["value_info"]["Value"] ;
            }

			throw new Exception("Something went wrong on the server side while geting last wight measurement " + response.Result);

        }

        public JournalEntry PushJournalEntry(string user_uri, string notification_type, string severity, string msg, string desc, long reference_id = -1) 
        {

            Console.WriteLine(string.Format("[StoreAPI] Attempting to send journal entry of type {0}, for user {1}, with msg {2} and desc {3}. Timestamp: {4}",
                notification_type, user_uri, msg, desc, ((int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString())) ;
            
            var obj = new JournalEntry()
            {
				user = user_uri,
                description = desc,
                message = msg,
                timestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                acknowledged = false,
                type = notification_type,
                severity = severity
            };

            if (reference_id != -1)
                obj.reference_id = reference_id;

            HttpContent content = new StringContent(JsonConvert.SerializeObject(obj, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore }));
			content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
			var response = new HttpClient().PostAsync(url + "/api/v1/journal_entries/", content);

            if (response.Result.IsSuccessStatusCode)
            {
                JournalEntry entry = JsonConvert.DeserializeObject<JournalEntry>(response.Result.Content.ReadAsStringAsync().Result);
                Console.WriteLine("[StoreAPI] Journal entry inserted: " + entry);

                return entry;
            }
            else
            {
                Console.WriteLine("[StoreAPI] Could not insert journal entry: " + obj + ". Reason: " + response.Result);
                return null;
            }
		}

        public string GetUserOfGateway(string gatewayURIPath)
        {
            Console.WriteLine("[StoreAPI] Retrieving the end-user URI of the owner of this gatewayURI: " + gatewayURIPath);

            var response = new HttpClient().GetAsync(url + gatewayURIPath);

            if (response.Result.IsSuccessStatusCode)
            {
                dynamic deserialized = JsonConvert.DeserializeObject<dynamic>(response.Result.Content.ReadAsStringAsync().Result);
                return deserialized["user"];
            }
            else
            {
                Console.WriteLine("[StoreAPI] Could not retrieve the gateway referenced by the URI " + (url + gatewayURIPath) + ". Reason: " + response.Result);
                return null;
            }    
        }


        public Tuple<string, string> GetUserLocale(string userURIPath, int userID)
        {
            Dictionary<string, string> timezoneMap = new Dictionary<string, string>()
            {
                { "en", "Europe/London" },
                { "ro", "Europe/Bucharest" },
                { "pl", "Europe/Warsaw" },
                { "dk", "Europe/Copenhagen" },
            };

            Console.WriteLine("Retrieving language and timezone for user: " + userURIPath);

            string queryPath = String.Format("/api/v1/enduserprofile/?limit=1&user={0}", userID);
            var response = new HttpClient().GetAsync(url + queryPath);

            if (response.Result.IsSuccessStatusCode)
            {
                dynamic deserialized = JsonConvert.DeserializeObject<dynamic>(response.Result.Content.ReadAsStringAsync().Result);
                string lang = deserialized["enduser_profiles"][0]["language"];

                if (timezoneMap.ContainsKey(lang))
                {
                    return new Tuple<string, string>(lang, timezoneMap[lang]);
                }
                else
                {
                    return new Tuple<string, string>("en", timezoneMap["en"]);
                }
            }
            else
            {
                Console.WriteLine("Could not retrieve locales for user referenced by the URI " + (url + userURIPath) + ". Reason: " + response.Result);
                return null;
            }
        }

        public dynamic GetUserData(string userURIPath)
        {
            var response = new HttpClient().GetAsync(url + userURIPath);
            if (response.Result.IsSuccessStatusCode)
            {
                dynamic deserialized = JsonConvert.DeserializeObject<dynamic>(response.Result.Content.ReadAsStringAsync().Result);
                return deserialized;
            }
            else
            {
                Console.WriteLine("[StoreAPI] Could not retrieve the user referenced by the URI " + (url + userURIPath) + ". Reason: " + response.Result);
                return null;
            }
        }
    }
}

using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace DSS.RMQ
{


    public class ValueInfo
    {
        public int val { get; set; }
    }

    public class JournalEntry
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int id { get; set; }
        public string user { get; set; }
        public string severity { get; set; }
        public long timestamp { get; set; }
        public string description { get; set; }
        public string message { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public long reference_id { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string resource_uri { get; set; }
        public string type { get; set; }
        public bool? acknowledged { get; set; }
    }

    public class StoreAPI
    {

        public string url { get; set; }

        public StoreAPI(string baseUrl)
        {
            this.url = baseUrl;
        }

        public void PushMeasurement(string json)
        {
            HttpContent content = new StringContent(json);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = new HttpClient().PostAsync(url + "/api/v1/measurement/", content);

            //Console.WriteLine("PUSH MEASUREMNT:" + response.Result);
            Console.WriteLine("Measurement inserted");

        }

        public bool AreLastNHeartRateCritical(int n, int low, int high)
        {

            //var urlVS = url + "/measurement/?limit=3&measurement_type=pulse&order_by=-timestamp&user=2&device=2";
            // we want to get all pulse related data for the end-user (user=2), not just the ones from device 2
            var urlVS = url + "/api/v1/measurement/?limit=3&measurement_type=pulse&order_by=-timestamp&user=2";

            var response = new HttpClient().GetAsync(urlVS);

            if (response.Result.IsSuccessStatusCode)
            {
                dynamic deserialized = JsonConvert.DeserializeObject<dynamic>(response.Result.Content.ReadAsStringAsync().Result);
                var isCritical = true;


                //Console.WriteLine(deserialized);

                if (n == deserialized["measurements"].Count)
                {
                    for (int i = 0; i < deserialized["measurements"].Count; i++)
                    {
                        var item = deserialized["measurements"][i];

                        var hr = (int)(item["value_info"]["value"] ?? item["value_info"]["Value"]);

                        Console.WriteLine("HR: " + hr);

                        if (hr > low && hr < high)
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
                var measurements = deserialized["measurements"];

                if (measurements.Count > 0) {
                    return measurements[0]["value_info"]["value"] ?? measurements[0]["value_info"]["Value"];
                }
                else {
                    return 0;
                }
            }

            throw new Exception("Something went wrong on the server side while geting last wight measurement " + response.Result);

        }

        public JournalEntry PushJournalEntry(string user_uri, string notification_type, string severity, string msg, string desc, long reference_id = 0)
        {

            //Console.WriteLine(string.Format("[StoreAPI] Attempting to send journal entry of type {0}, for user {1}, with msg {2} and desc {3}. Timestamp: {4}",
            //    notification_type, user_uri, msg, desc, ((int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString()));
            Console.WriteLine("[StoreAPI] Sending journal entry of type " + notification_type + " for user " + user_uri + " with msg: " + msg);
            var obj = new JournalEntry()
            {
                user = user_uri,
                description = desc,
                message = msg,
                timestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                //acknowledged = false,
                type = notification_type,
                severity = severity
            };

            if (reference_id != 0)
                obj.reference_id = reference_id;

            HttpContent content = new StringContent(JsonConvert.SerializeObject(obj));
            //Console.WriteLine(content.ReadAsStringAsync().Result);

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


        public string GetLang(string userURI) 
        {
            Console.WriteLine("Retrieving language for user: " + userURI);

            var response = new HttpClient().GetAsync(url + userURI);

            if (response.Result.IsSuccessStatusCode)
            {
                dynamic deserialized = JsonConvert.DeserializeObject<dynamic>(response.Result.Content.ReadAsStringAsync().Result);
                return deserialized["enduser_profile"]["language"].ToString().ToUpper();

            }
            else
            {
                Console.WriteLine("Could not retrieve locales for user referenced by the URI " + (url + userURI) + ". Reason: " + response.Result);
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
                
                /*
                { "en", "GMT Standard Time" },
                { "ro", "E. Europe Standard Time" },
                { "pl", "Central European Standard Time" },
                { "dk", "Romance Standard Time" },
                */
            };

            Console.WriteLine("Retrieving language and timezone for user: " + userURIPath);

            string queryPath = String.Format("/api/v1/enduserprofile/?limit=1&user={0}", userID);
            var response = new HttpClient().GetAsync(url + queryPath);

            if (response.Result.IsSuccessStatusCode)
            {
                dynamic deserialized = JsonConvert.DeserializeObject<dynamic>(response.Result.Content.ReadAsStringAsync().Result);
                string lang = deserialized["enduser_profile"]["language"];

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

        public dynamic GetCaregivers(string userURIPath)
        {
            var response = new HttpClient().GetAsync(url + userURIPath);
            if (response.Result.IsSuccessStatusCode)
            {
                dynamic deserialized = JsonConvert.DeserializeObject<dynamic>(response.Result.Content.ReadAsStringAsync().Result);
                return deserialized["enduser_profile"]["caregivers"];
            }
            else
            {
                Console.WriteLine("[StoreAPI] Could not retrieve the user referenced by the URI " + (url + userURIPath) + ". Reason: " + response.Result);
                return null;
            }

        }

        public bool CheckForMeasuremntInLastNMinutes(string type, int min, int userId) {


            Console.WriteLine("Check for mesurements in last 6 minutes");

            DateTime now = DateTime.UtcNow.AddMinutes(-min);
            long timeStamp = (long)now.Date.Add(new TimeSpan(0, 0, 0)).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;


            //1477413397
            string queryPath = String.Format("/api/v1/measurement/?measurement_type={0}&user={1}&timestamp__gte={2}", type, userId, timeStamp);


            var response = new HttpClient().GetAsync(url + queryPath);
            if (response.Result.IsSuccessStatusCode)
            {
                dynamic deserialized = JsonConvert.DeserializeObject<dynamic>(response.Result.Content.ReadAsStringAsync().Result);


                Console.WriteLine(deserialized);

                if (((JArray)deserialized.measurements).Count() == 0)
                    return false;

                return true;
            }
            else
            {
                Console.WriteLine("[StoreAPI] Could not retrieve the user referenced by the URI " + (url + queryPath) + ". Reason: " + response.Result);
                return false;
            }
        }

        public int GetUserStepCount(string userURIPath, long startTs, long endTs)
        {
            int userID = GetIdFromURI(userURIPath);

            string queryPath = String.Format("/api/v1/measurement/?measurement_type=steps&user={0}&timestamp__gte={1}&timestamp__lte={2}&order_by=-timestamp", userID, startTs, endTs);

            var response = new HttpClient().GetAsync(url + queryPath);
            if (response.Result.IsSuccessStatusCode)
            {
                dynamic deserialized = JsonConvert.DeserializeObject<dynamic>(response.Result.Content.ReadAsStringAsync().Result);

                JArray measurements = (JArray)deserialized["measurements"];
                int totalCount = 0;

                for (int i = 0; i < measurements.Count; i++)
                {
                    var meas = (JObject)measurements[i];
                    totalCount += (int)meas["value_info"]["value"];
                }

                return totalCount;
            }

            return 0;
        }

        public int GetIdFromURI(string uri)
        {
            string idStr = uri.TrimEnd('/').Split('/').Last();

            int id = Int32.Parse(idStr);
            return id;
        }

        public void PatchJournalEntry(string id, bool ack)
        {

            HttpContent content = new StringContent(JsonConvert.SerializeObject(new { acknowledged = ack }));
            var response = new HttpClient().PatchAsync(new Uri(url + "/api/v1/journal_entries/" + id + "/"), content);

        }

        public JournalEntry GetJournalEntryById(string id)
        {
            Console.WriteLine("Get yournal entry by id" + id);

            var response = new HttpClient().GetAsync(url + "/api/v1/journal_entries/" + id +"/");

            if (response.Result.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<JournalEntry>(response.Result.Content.ReadAsStringAsync().Result);

            }
            else
            {

                throw new Exception("Couldn't get a journal entry with the ID  of " + id);
            }
        }
    }


    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, Uri requestUri, HttpContent iContent)
        {
            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = iContent
            };

            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                response = await client.SendAsync(request);
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine("ERROR: " + e.ToString());
            }

            return response;
        }
    }
}
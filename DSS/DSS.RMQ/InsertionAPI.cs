using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace DSS.RMQ
{
    public class InsertionAPI : IInsertionAPI
    {
		public string url { get; set; }

		public InsertionAPI(string baseUrl)
		{
			this.url = baseUrl;
		}

		public void InsertEvent(string json)
		{
			HttpContent content = new StringContent(json);
			content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

			var response = new HttpClient().PostAsync(url + "/events/", content);

			//Console.WriteLine("INSERT EVENT:" + response.Result);
            Console.WriteLine("Event inserted");
		}


        public void InsertPushNotification(string json){

			HttpContent content = new StringContent(json);
			content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

			var response = new HttpClient().PostAsync(url + "/push_notifications/", content);

			//Console.WriteLine("INSERT NOTIFICATION:" + response.Result);
			Console.WriteLine("Notification inserted");

		} 

        public void InsertPushNotification(string msg, int userId){
        
            InsertPushNotification(JsonConvert.SerializeObject(new PushNotification() { message = msg, user_id = userId }));
            Console.WriteLine("Notification with message: " + msg + " inserted for user: " + userId);
        }
	}
}

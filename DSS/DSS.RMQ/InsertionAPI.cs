using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace DSS.RMQ.INS
{


	public class User
	{
		public string name { get; set; }
		public string uri { get; set; }
	}

	public class Room
	{
		public string name { get; set; }
	}

	public class Value
	{
		public User user { get; set; }
		public Room room { get; set; }
	}

	public class Content
	{
		public string name { get; set; }
		public string value_type { get; set; }

		[JsonProperty("value")]
		public Value VALUE { get; set; }
        public float num_value { get; set; }
	}

	public class TemporalValidity
	{
		public int start_ts { get; set; }
		public int end_ts { get; set; }
	}

	public class Annotations
	{
		public int timestamp { get; set; }
		public List<string> source { get; set; }
		public int certainty { get; set; }
		public TemporalValidity temporal_validity { get; set; }
	}

	public class Event
	{
		public string category { get; set; }
		public Content content { get; set; }
		public Annotations annotations { get; set; }
	}


	public class PushNotification
	{
		public int user_id { get; set; }
		public string message { get; set; }
	}


	/// <summary>
	/// //////////////////////////////////////////////////
	/// </summary>

	public class InsertionAPI
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

			Console.WriteLine("INSERT EVENT:" + response.Result);
		}


        public void InsertPushNotification(string json){

			HttpContent content = new StringContent(json);
			content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

			var response = new HttpClient().PostAsync(url + "/push_notifications/", content);

			Console.WriteLine("INSERT NOTIFICATION:" + response.Result);

        } 
	}
}

using System;
using System.Net.Http;

namespace DSS.RMQ
{
    public class RmqAPI
    {

        public string url { get; set; }
        public HttpClient client { get; set; }

        public RmqAPI(string baseUrl)
        {
            this.url = baseUrl;
			client = new HttpClient();
		}

        public void PushEvent(string json )
        {

            HttpContent content = new StringContent(json);  
            var response = client.PostAsync( url + "/events/", content);

			Console.WriteLine(response.Result);
        }

        public void PushNotification(string json) 
        {


            //TODO: THIS IS HORRIBLE AAAAAAAAAAAAAAA
            try
            {
                var response = client.PostAsync(url + "/push_notifications/", new StringContent( json));
               Console.WriteLine(response.Result);
            }
            catch (Exception )
            {
                PushNotification(json);
            }

			
        }
    }
}

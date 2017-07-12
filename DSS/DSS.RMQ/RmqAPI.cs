using System;
using System.Net.Http;

namespace DSS.RMQ
{
    public class RmqAPI
    {

        public string url
        {
            get;
            set;
        }

        public HttpClient client
        {
            get;
            set;
        }



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
    }
}

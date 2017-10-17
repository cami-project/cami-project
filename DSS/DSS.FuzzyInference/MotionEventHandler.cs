using DSS.Delegate;
using DSS.RMQ;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.FuzzyInference
{
    public class MotionEventHandler : IRouterHandler
    {
        private StoreAPI storeAPI;
        private RMQ.INS.InsertionAPI insertionAPI;

        /**
         * Map that stores the last activation timestamp of the motion sensor for each user uri
         */
        private Dictionary<string, long> lastActivationMap;

        public string Name => "EVENT";
        
        public MotionEventHandler()
        {
            storeAPI = new StoreAPI("http://cami-store:8008");
            insertionAPI = new RMQ.INS.InsertionAPI("http://cami-insertion:8010/api/v1/insertion");

            lastActivationMap = new Dictionary<string, long>();
        }

        public void Handle(string json)
        {
            Console.WriteLine("MOTION event handler invoked ...");
            var eventObj = JsonConvert.DeserializeObject<Event>(json);

            if (eventObj.category.ToLower() == "user_environment")
            {
                // if we are dealing with a presence sensor
                if (eventObj.content.name == "presence")
                {
                    // see if it is a sensor activation
                    if ((bool)eventObj.content.val["alarm_motion"] == true)
                    {
                        // retrieve the gateway and sensor URI from the source annotations
                        var gatewayURIPath = eventObj.annotations.source["gateway"];
                        var deviceURIPath = eventObj.annotations.source["sensor"];

                        // make a call to the store API to get the user from the gateway
                        var userURIPath = storeAPI.getUserOfGateway(gatewayURIPath);

                        if (userURIPath != null)
                        {
                            int userID = GetIdFromURI(userURIPath);

                        }
                        else
                        {
                            Console.WriteLine("[MotionEventHandler] Skipping motion event handling since no user can be retrieved for gateway: " + gatewayURIPath);
                        }
                    }
                }
            }
        }

        private int GetIdFromURI(string uri)
        {
            string idStr = uri.TrimEnd('/').Split('/').Last();

            int id = Int32.Parse(idStr);
            return id;
        }
    }
}

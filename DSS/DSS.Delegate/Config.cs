using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace DSS.Delegate
{
    public class Config
    {
        public string Url { get; }
        public string [] Channels { get; }
        public Dictionary<string, string[]> ChannelsEvents { get; }


        public Config(string path, Router<Event> router ,IRouterHandler<Event> [] handlers)
        {
            path = path.Contains(".json") ? path : path + ".json";
            var config = JsonConvert.DeserializeObject<Details>(File.ReadAllText(path));

            Url = config.url + ":" + config.port;
            Channels = config.channels.ToList().Select(x => x.name).ToArray();
            ChannelsEvents = config.channels.ToDictionary(x => x.name, x => x.events);


			foreach (var channel in Channels)
			{
                router.RegisterChannel(channel, handlers.Last(x => x.Name == channel));
			}

			foreach (var channel in ChannelsEvents.Keys)
			{
				foreach (var e in ChannelsEvents[channel])
				{
					router.RegisterEvent(channel, new Event(e, e,  "", "Device"));
				}
			}
        }

        private class Details 
        {
            public string url;
            public string port;
            public Channel [] channels;

            public override string ToString()
            {
                return string.Format("Url: {0} , Port: {1}", url, port);
            }

            internal class Channel
            {
                public string name;
                public string[] events;

                public override string ToString()
                {
                    return string.Format("Channnel: {0} and count: {1}", name, events.Length);
                }
            }
        }
    }
}

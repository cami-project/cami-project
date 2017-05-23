using System;
using System.Collections.Generic;
using System.Linq;

namespace DSS.Delegate
{
    public class Router<T>
    {
		private Dictionary<string, Channel<T>> Channels;
        private Dictionary<string, IRouterHandler<T>> Handlers;

        public Router()
        {
            this.Channels = new Dictionary<string, Channel<T>>();
            this.Handlers = new Dictionary<string, IRouterHandler<T>>();
        }

        public void RegisterChannel(string name, IRouterHandler<T> handler)
        {
            Channels.Add(name.ToUpper(), new Channel<T>(name.ToUpper()));
            Handlers.Add(name.ToUpper(), handler);

        }
        public void RegisterEvent(string channel, T obj)
        {
            channel = channel.ToUpper();

            if(!Channels.ContainsKey(channel))
            {
                throw new Exception(string.Format("Channel {0} is not present inside of the router", channel));
            }

            Channels[channel].Push(obj);
        }

        public void Handle(T obj)
        {
            foreach (var channel in Channels.Values)
            {
                if(channel.Contains(obj))
                {
                   
                    Handlers[channel.Name].Handle(obj);
                }
            }
        }

    }
}

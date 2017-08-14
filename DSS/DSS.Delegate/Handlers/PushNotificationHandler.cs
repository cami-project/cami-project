using System;
using DSS.RMQ;
using Newtonsoft.Json;

namespace DSS.Delegate
{
	public class PushNotificationHandler<T> : IRouterHandler<T>
	{
		private IWriteToBroker<T> MsgBroker;

		public PushNotificationHandler(IWriteToBroker<T> msgBroker)
		{
			MsgBroker = msgBroker;
		}

		public string Name => "NOTIFICATION";

		public void Handle(T obj)
		{
			Console.WriteLine("Handled by NOTIFICATION channel: " + obj);
			MsgBroker.Write(JsonConvert.SerializeObject(obj));
		}
	}
}

using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace DSS.RMQ
{
    public class RmqWriter<T> : IDisposable, IWriteToBroker<T>
    {
		public IConnection connection { get; private set; }
		public IModel channel { get; private set; }

		private string writeQueue { get; set; }

		public RmqWriter(string url, string username, string pass, string queue)
		{
			var factory = new ConnectionFactory() { Uri = url, UserName = username, Password = pass };

			connection = factory.CreateConnection();
			channel = connection.CreateModel();

            channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: null);

			writeQueue = queue;
		}


		public void Dispose()
		{
			channel.Dispose();
			connection.Dispose();
		}

		public void Write(string msg)
		{
			channel.BasicPublish(exchange: "", routingKey: writeQueue, basicProperties: null, body: Encoding.UTF8.GetBytes(msg));
		}

    }
}

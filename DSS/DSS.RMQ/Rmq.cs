using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DSS.RMQ
{

    public class RmqConfig 
    {

        public string url
        {
            get;
            set;
  
        }
        public string username
        {
            get;
            set;
        }
        public string password
        {
            get;
            set;
        }
        public string exchange
        {
            get;
            set;
        }

        public RmqConfig(string url, string user, string pass, string exchange)    
        {
            this.url = url;
            this.username = user;
            this.password = pass;
            this.exchange = exchange;
        }


    }

    public class RmqExchange 
    {

        public RmqExchange(RmqConfig config)
        {
            var factory = new ConnectionFactory() { Uri = config.url, UserName = config.username, Password = config.password };
			var connection = factory.CreateConnection();
			var channel = connection.CreateModel();
			{

				channel.ExchangeDeclare(exchange: config.exchange, type: "topic", durable: true);

				var queueName = channel.QueueDeclare().QueueName;


				channel.QueueBind(queue: queueName,
								  exchange: config.exchange,
								  routingKey: "event.*");

				var consumer = new EventingBasicConsumer(channel);


				consumer.Received += (model, ea) =>
				{
					Console.WriteLine("Rmq response");

					//onRecieve(Encoding.UTF8.GetString(ea.Body));
				};
				channel.BasicConsume(queue: queueName,
				noAck: true,
				consumer: consumer);

			}
		
        }


        public RmqExchange(string url, string username, string pass, string exchange, Action<string> onRecieve)
        {
			var factory = new ConnectionFactory() { Uri = url, UserName = username, Password = pass };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
		
                
				channel.ExchangeDeclare(exchange: exchange, type: "topic", durable: true);

				var queueName = channel.QueueDeclare().QueueName;


				channel.QueueBind(queue: queueName,
								  exchange: exchange,
								  routingKey: "event.*");

				var consumer = new EventingBasicConsumer(channel);


				consumer.Received += (model, ea) =>
				{
					Console.WriteLine("Rmq response");

					onRecieve(Encoding.UTF8.GetString(ea.Body));
				};
				channel.BasicConsume(queue: queueName,
				noAck: true,
				consumer: consumer);




			
		}
    }




    public class Rmq<T> : IDisposable, IWriteToBroker<T>
	{
		public IConnection connection { get; private set; }
		public IModel channel { get; private set; }

		private string writeQueue { get; set; }
        private string readQueue { get; set; }


		public Rmq(string url, string username, string pass, string queue, Action<string> onRecieve)
		{
			var factory = new ConnectionFactory() { Uri = url, UserName = username, Password = pass };

			connection = factory.CreateConnection();
			channel = connection.CreateModel();

			channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);

            writeQueue = queue;

			var consumer = new EventingBasicConsumer(channel);
			consumer.Received += (model, ea) => onRecieve(Encoding.UTF8.GetString(ea.Body));
            channel.BasicConsume(queue: writeQueue, noAck: true, consumer: consumer);

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

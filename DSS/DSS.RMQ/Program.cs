using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Events;
using System.Threading;

namespace DSS.RMQ
{
	class MainClass
	{
		public static void Main()
		{
            
            var rmq = new Rmq("amqp://cami:cami@141.85.241.224:5673/cami", "cami", "cami");

            Read(rmq);
            Write(rmq, "porukica jedan");
            Write(rmq, "porukica zwei");


            Console.Read();

            rmq.Dispose();

		}

        private static void Write(Rmq rmq, string msg)
        {
            try
			{
			    rmq.channel.QueueDeclare(queue: "test-queue",durable: false,exclusive: false,autoDelete: false, arguments: null);
                rmq.channel.BasicPublish(exchange: "", routingKey: "test-queue", basicProperties: null, body: Encoding.UTF8.GetBytes(msg));
                    
                Console.Write("\nMsg sent to the queue : {0}", msg);
			}
			catch (BrokerUnreachableException ex)
			{
				Console.WriteLine(ex.InnerException);
			}
        }


        private static void Read(Rmq rmqReader )
        {
		   
            try
			{
                rmqReader.channel.QueueDeclare(queue: "test-queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(rmqReader.channel);
				consumer.Received += (model, ea) => Console.WriteLine("\nReceived: {0}", Encoding.UTF8.GetString(ea.Body));
                rmqReader.channel.BasicConsume(queue: "test-queue", noAck: true, consumer: consumer);

			}
			catch (BrokerUnreachableException ex)
			{
				Console.WriteLine(ex.InnerException);
			}
        }


        public class Rmq : IDisposable
        {
            public IConnection connection { get; private set; }
            public IModel channel { get; private set; }


            public Rmq(string url, string username, string pass)
            {
                var factory = new ConnectionFactory() { Uri = url, UserName = username, Password = pass };
				
                connection = factory.CreateConnection();
				channel = connection.CreateModel();
            }

            public Rmq(ConnectionFactory factory)
            {
                connection = factory.CreateConnection();
                channel = connection.CreateModel();
            }

            public void Dispose()
            {
				channel.Dispose();
                connection.Dispose();
            }
        }

    }
}

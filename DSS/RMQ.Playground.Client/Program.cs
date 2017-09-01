using System;
using System.Text;
using RabbitMQ.Client;

namespace RMQ.Playground.Client
{
    class MainClass
    {
        public static void Main(string[] args)
        {

            Console.WriteLine("Client invoked");
            var factory = new ConnectionFactory() { HostName = "localhost" };
			using (var connection = factory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				channel.ExchangeDeclare(exchange: "amq.topic",
                                        type: "topic", durable: true);

                var message = "pojedi govance";
				var body = Encoding.UTF8.GetBytes(message);
				channel.BasicPublish(exchange: "amq.topic",
									 routingKey: "event.one",
									 basicProperties: 
                                     null,
									 body: body);
				Console.WriteLine(" [x] Sent '", message);
			}


			Console.WriteLine(" Press [enter] to exit.");
			Console.ReadLine();		
        
        }
    }
}

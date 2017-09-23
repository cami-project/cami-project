using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RMQ.Playgorund.Receiver
{
    class MainClass
    {
        public static void Main(string[] args)
        {

            Console.WriteLine("Client bruh");

            var factory = new ConnectionFactory() { HostName = "localhost" };
			var connection = factory.CreateConnection();
			var channel = connection.CreateModel();


			channel.ExchangeDeclare(exchange: "amq.topic", type: "topic", durable: true);
            var queueName = "hello" ;//channel.QueueDeclare();


			channel.QueueBind(queue: queueName,
                              exchange: "amq.topic",
                              routingKey: "measurement.*",
                              arguments: null);
            
			var consumer = new EventingBasicConsumer(channel);


			consumer.Received += (model, ea) =>
			{
                Console.WriteLine("Rmq response : " +Encoding.UTF8.GetString(ea.Body));

				//onRecieve(Encoding.UTF8.GetString(ea.Body));
			};
			channel.BasicConsume(queue: queueName,
            noAck: true,
                                 consumer: consumer);     


            // Console.ReadLine();
            while (true)
            {

            }
        
        }
    }
}

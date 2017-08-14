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
            
            var rmq = new Rmq("amqp://cami:cami@141.85.241.224:5673/cami",
                              "cami", 
                              "cami", 
                              "test-queue", 
                              (result)=> Console.WriteLine("RECIEVED: " + result));


            rmq.Write("Hello from the updated version!");

            Console.Read();

            rmq.Dispose();

		}

    }
}

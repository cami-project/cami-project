using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.RMQ
{
    class CeleryWorker
    {
        private IModel rmqChannel;

        public CeleryWorker(IModel rmqChannel)
        {
            this.rmqChannel = rmqChannel;
        }

        public void SendTask(string taskName, string exchangeName, string measurement_json)
        {

        }
    }
}

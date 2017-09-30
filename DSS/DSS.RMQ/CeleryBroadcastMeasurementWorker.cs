using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.RMQ
{
    public class CeleryBroadcastMeasurementWorker
    {
        private IModel rmqChannel;

        public CeleryBroadcastMeasurementWorker(string brokerUrl)
        {
            var factory = new ConnectionFactory() { Uri = brokerUrl };
            var connection = factory.CreateConnection();

            rmqChannel = connection.CreateModel();
        }

        public void SendTask(string taskName, string exchangeName, string measurement_json)
        {
            IDictionary<string, object> headers = new Dictionary<string, object>();
            Guid id = Guid.NewGuid();

            headers.Add("task", taskName);
            headers.Add("id", id.ToString());

            IBasicProperties props = rmqChannel.CreateBasicProperties();
            props.Headers = headers;
            props.CorrelationId = (string)headers["id"];
            props.ContentEncoding = "utf-8";
            props.ContentType = "application/json";

            var body = Encoding.UTF8.GetBytes(measurement_json);

            rmqChannel.BasicPublish(
                exchange: exchangeName,
                routingKey: "",
                basicProperties: props,
                body: body);
        }
    }
}

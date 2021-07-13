using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace Tutorial.RabbitMQ.Console.RPCClient
{
    class RPCClient
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string queueName = "rpc_queue";
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;
        private readonly string CorrelationId;

        public RPCClient()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);

            props = channel.CreateBasicProperties();
            CorrelationId = Guid.NewGuid().ToString();
            props.CorrelationId = CorrelationId;
            props.ReplyTo = replyQueueName;

            consumer.Received += Consumer_Received;
        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            if (e.BasicProperties.CorrelationId == CorrelationId)
            {
                respQueue.Add(response);
            }
        }

        public string Call(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: queueName, 
                                 basicProperties: props,
                                 body: messageBytes);

            channel.BasicConsume(consumer: consumer,
                                 queue: replyQueueName,
                                 autoAck: true);

            return respQueue.Take();
        }

        public void Close()
        {
            connection.Close();
        }
    }

    public class Rpc
    {
        static void Main(string[] args)
        {
            var rpcClient = new RPCClient();

            System.Console.WriteLine($"{DateTime.Now}: Press Requesting fib(30).");
            var response = rpcClient.Call("4");
            System.Console.WriteLine($"{DateTime.Now}: Got '{response}'");

            rpcClient.Close();
        }
    }
}

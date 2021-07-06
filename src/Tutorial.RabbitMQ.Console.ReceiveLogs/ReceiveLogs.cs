using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Tutorial.RabbitMQ.Console.ReceiveLogs
{
    class ReceiveLogs
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var exchangeName = "logs";

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: exchangeName,
                                        type: ExchangeType.Fanout);

                var queueName = channel.QueueDeclare().QueueName;
                
                channel.QueueBind(queue: queueName,
                                  exchange: exchangeName,
                                  routingKey: string.Empty);

                System.Console.WriteLine($"{DateTime.Now}: Waiting for logs.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += Consumer_Received;

                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                System.Console.WriteLine($"{DateTime.Now}: Press [enter] to exit.");
                System.Console.ReadLine();
            }
        }

        private static void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            System.Console.WriteLine($"{DateTime.Now}: Received '{message}'");
        }
    }
}

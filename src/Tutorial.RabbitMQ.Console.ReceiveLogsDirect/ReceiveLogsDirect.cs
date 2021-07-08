using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Tutorial.RabbitMQ.Console.ReceiveLogsDirect
{
    class ReceiveLogsDirect
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var exchangeName = "direct_logs";

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: exchangeName,
                                        type: ExchangeType.Direct);

                var queueName = channel.QueueDeclare().QueueName;

                if (args.Length < 1)
                {
                    System.Console.Error.WriteLine($"{DateTime.Now}: Usage: {Environment.GetCommandLineArgs()[0]} [info] [warning] [error].");
                    
                    System.Console.WriteLine($"{DateTime.Now}: Press [enter] to exit.");
                    System.Console.ReadLine();
                    
                    Environment.ExitCode = 1;
                    return;
                }

                foreach (var severity in args)
                {
                    channel.QueueBind(queue: queueName,
                                      exchange: exchangeName,
                                      routingKey: severity);
                }

                System.Console.WriteLine($"{DateTime.Now}: Waiting for messages.");

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
            var routingKey = e.RoutingKey;

            System.Console.WriteLine($"{DateTime.Now}: Received '{routingKey}':'{message}'");
        }
    }
}

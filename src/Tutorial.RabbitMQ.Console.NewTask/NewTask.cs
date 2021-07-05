using RabbitMQ.Client;
using System;
using System.Text;

namespace Tutorial.RabbitMQ.Console.NewTask
{
    class NewTask
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var queueName = "task_queue";

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                string message = GetMessage(args);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: string.Empty,
                                     routingKey: queueName,
                                     basicProperties: properties,
                                     body: body);

                System.Console.WriteLine($"{DateTime.Now}: Sent '{message}'");
            }

            System.Console.WriteLine($"{DateTime.Now}: Press [enter] to exit.");
            System.Console.ReadLine();
        }

        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
        }
    }
}

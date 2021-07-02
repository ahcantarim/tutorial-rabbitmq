using RabbitMQ.Client;
using System;
using System.Text;

namespace Tutorial.RabbitMQ.Console.Send
{
    class Send
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var queueName = "hello";

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queueName, false, false, false, null);

                string message = $"Hello World at {DateTime.Now}!";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(string.Empty, queueName, null, body);

                System.Console.WriteLine($" [x] Sent '{message}'");
            }

            System.Console.WriteLine($" Press [enter] to exit.");
            System.Console.ReadLine();
        }
    }
}

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
                channel.QueueDeclare(queue: queueName, 
                                     durable: false, 
                                     exclusive: false, 
                                     autoDelete: false, 
                                     arguments: null);

                string message = $"{DateTime.Now}: Hello World!";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: string.Empty, 
                                     routingKey: queueName,
                                     basicProperties: null, 
                                     body: body);

                System.Console.WriteLine($"{DateTime.Now}: Sent '{message}'");
            }

            System.Console.WriteLine($"{DateTime.Now}: Press [enter] to exit.");
            System.Console.ReadLine();
        }
    }
}

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace Tutorial.RabbitMQ.Console.Worker
{
    class Worker
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

            System.Console.WriteLine($"{DateTime.Now}: Received '{message}'");

            int dots = message.Split('.').Length - 1;
            Thread.Sleep(dots * 1000);

            System.Console.WriteLine($"{DateTime.Now}: Done.");
        }
    }
}

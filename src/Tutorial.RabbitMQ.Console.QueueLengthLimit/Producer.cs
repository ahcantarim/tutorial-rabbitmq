using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tutorial.RabbitMQ.Console.QueueLengthLimit
{
    class Producer
    {
        static void Main(string[] args)
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                var queueName = "limited_queue";
                var exchangeName = "limited.nack";

                var arguments = new Dictionary<string, object>();
                arguments.Add("x-max-length", 5);
                arguments.Add("x-overflow", "reject-publish-dlx");  // "drop-head" | "reject-publish" | "reject-publish-dlx"
                arguments.Add("x-dead-letter-exchange", exchangeName);

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: exchangeName,
                                        type: ExchangeType.Fanout);

                    channel.QueueDeclare(queue: queueName,
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: arguments);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    for (int i = 0; i < 10; i++)
                    {
                        string message = $"{DateTime.Now}: Hello #{i + 1}";
                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish(exchange: string.Empty,
                                             routingKey: queueName,
                                             basicProperties: properties,
                                             body: body);

                        System.Console.WriteLine($"{DateTime.Now}: Sent '{message}'");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{DateTime.Now}: ERRO: {ex.Message}.");
                throw;
            }
            finally
            {
                System.Console.WriteLine($"{DateTime.Now}: Press [enter] to exit.");
                System.Console.ReadLine();
            }
        }
    }
}

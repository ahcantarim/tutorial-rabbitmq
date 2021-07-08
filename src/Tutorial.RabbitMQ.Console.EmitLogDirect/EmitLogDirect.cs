using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

namespace Tutorial.RabbitMQ.Console.EmitLogDirect
{
    class EmitLogDirect
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

                var severity = (args.Length > 0) ? args[0] : "info";

                var message = (args.Length > 1)
                              ? string.Join(" ", args.Skip(1).ToArray())
                              : "Hello World!";

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: exchangeName,
                                     routingKey: severity,
                                     basicProperties: null,
                                     body: body);

                System.Console.WriteLine($"{DateTime.Now}: Sent '{severity}':'{message}'");
            }

            System.Console.WriteLine($"{DateTime.Now}: Press [enter] to exit.");
            System.Console.ReadLine();
        }
    }
}

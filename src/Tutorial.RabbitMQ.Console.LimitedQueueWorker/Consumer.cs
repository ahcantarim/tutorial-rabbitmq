using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Tutorial.RabbitMQ.Console.LimitedQueueWorker
{
    class Consumer
    {
        static void Main(string[] args)
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
                channel.QueueDeclare(queue: queueName,
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: arguments);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                System.Console.WriteLine($"{DateTime.Now}: Waiting for messages.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += Consumer_Received;

                // Nota: o valor do parâmetro `autoAck` foi alterado para `false`, visando realizar manualmente a confirmação/rejeição da mensagem recebida.
                channel.BasicConsume(queue: queueName,
                                     autoAck: false,
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

            // Nota: é possível acessar o canal aqui através do `((EventingBasicConsumer)sender).Model`.
            var channel = ((EventingBasicConsumer)sender).Model;
            channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
        }
    }
}

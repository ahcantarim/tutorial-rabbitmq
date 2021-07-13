using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Tutorial.RabbitMQ.Console.RPCServer
{
    class RPCServer
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var queueName = "rpc_queue";

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                channel.BasicQos(0, 1, false);

                var consumer = new EventingBasicConsumer(channel);

                channel.BasicConsume(queue: queueName,
                                     autoAck: false,
                                     consumer: consumer);

                System.Console.WriteLine($"{DateTime.Now}: Awaiting RPC requests.");

                consumer.Received += Consumer_Received;

                System.Console.WriteLine($"{DateTime.Now}: Press [enter] to exit.");
                System.Console.ReadLine();
            }
        }

        private static void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var channel = ((EventingBasicConsumer)sender).Model;
            string response = null;

            var body = e.Body.ToArray();
            var props = e.BasicProperties;
            var replyProps = channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            try
            {
                var message = Encoding.UTF8.GetString(body);
                int n = int.Parse(message);

                System.Console.WriteLine($"{DateTime.Now}: fib({message})");
                response = fib(n).ToString();

            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"{DateTime.Now}: ERROR {ex.Message}");
                response = string.Empty;
            }
            finally
            {
                var responseBytes = Encoding.UTF8.GetBytes(response);

                channel.BasicPublish(exchange: string.Empty,
                                     routingKey: props.ReplyTo,
                                     basicProperties: replyProps,
                                     body: responseBytes);

                channel.BasicAck(deliveryTag: e.DeliveryTag,
                                 multiple: false);
            }
        }

        /// <summary>
        /// Assumes only valid positive integer input.
        /// Don't expect this one to work for big numbers, and it's
        /// probably the slowest recursive implementation possible.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static int fib(int n)
        {
            if (n == 0 || n == 1)
            {
                return n;
            }

            return fib(n - 1) + fib(n - 2);
        }
    }
}

using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

namespace Tutorial.RabbitMQ.Console.LoadTest.Client
{
    class Client
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672, UserName = "guest", Password = "guest" };
            var queueName = "load_test";

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                connection.ConnectionBlocked += Connection_ConnectionBlocked;
                connection.ConnectionUnblocked += Connection_ConnectionUnblocked;

                channel.QueueDeclare(queueName, true, false, false, null);

                string message = string.Empty;
                byte[] body = null;
                var rnd = new Random();

                for (int i = 0; i <= 1_000_000; i++)
                {
                    var dotsCount = rnd.Next(1, 6);
                    var dotsSequence = Enumerable.Repeat(".", dotsCount);

                    message = $"Mensagem '{i}' vai demorar {dotsCount} segundos{string.Join("", dotsSequence)}";
                    body = Encoding.UTF8.GetBytes(message);

                    var props = channel.CreateBasicProperties();
                    props.Persistent = true;

                    channel.BasicPublish(string.Empty, queueName, props, body);

                    System.Console.WriteLine($"[x] {DateTime.Now} - Enviando mensagem '{message}'");
                }
            }

            System.Console.WriteLine($"[x] {DateTime.Now} - Pressione [ENTER] para sair.");
            System.Console.ReadLine();
        }

        private static void Connection_ConnectionUnblocked(object sender, EventArgs e)
        {
            System.Console.WriteLine($"[|] {DateTime.Now} - A CONEXÃO ESTÁ BLOQUEADA!");
            System.Console.ReadLine();
        }

        private static void Connection_ConnectionBlocked(object sender, global::RabbitMQ.Client.Events.ConnectionBlockedEventArgs e)
        {
            System.Console.WriteLine($"[-] {DateTime.Now} - A CONEXÃO ESTÁ desBLOQUEADA!");
            System.Console.ReadLine();
        }
    }
}

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace Tutorial.RabbitMQ.Console.LoadTest.Server
{
    class Server
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672, UserName = "guest", Password = "guest" };
            var queueName = "load_test";

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queueName, true, false, false, null);
                channel.BasicQos(0, 10000, false);

                System.Console.WriteLine($"[x] {DateTime.Now} - Aguardando mensagens...");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    System.Console.WriteLine($"[.] {DateTime.Now} - Mensagem recebida '{message}'");
                    int dots = message.Split('.').Length - 1;
                    //Thread.Sleep(dots * 1000);
                    System.Console.WriteLine($"[x] {DateTime.Now} - Mensagem processada '{message}'");

                    channel.BasicAck(ea.DeliveryTag, false);
                };

                channel.BasicConsume(queueName, false, consumer);

                System.Console.WriteLine($"[x] {DateTime.Now} - Pressione [ENTER] para sair.");
                System.Console.ReadLine();
            }
        }
    }
}

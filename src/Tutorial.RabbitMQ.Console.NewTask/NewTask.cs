using RabbitMQ.Client;
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
                channel.QueueDeclare(queueName, false, false, false, null);

                string message = GetMessage(args);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(string.Empty, queueName, properties, body);

                System.Console.WriteLine($" [x] Sent '{message}'");
            }

            System.Console.WriteLine($" Press [enter] to exit.");
            System.Console.ReadLine();
        }

        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
        }
    }
}

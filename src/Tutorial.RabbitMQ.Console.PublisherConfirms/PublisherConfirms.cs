using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tutorial.RabbitMQ.Console.PublisherConfirms
{
    class PublisherConfirms
    {
        private const int MESSAGE_COUNT = 50_000;

        static void Main(string[] args)
        {
            PublishMessagesIndividually();
            //PublishMessagesInBatch();
            HandlePublishConfirmsAsynchronously();
        }

        private static IConnection CreateConnection()
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            return factory.CreateConnection();
        }

        private static void PublishMessagesIndividually()
        {
            using (var connection = CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Declare a server-named queue
                var queueName = channel.QueueDeclare().QueueName;
                channel.ConfirmSelect();

                var timer = new Stopwatch();
                timer.Start();

                for (int i = 0; i < MESSAGE_COUNT; i++)
                {
                    var body = Encoding.UTF8.GetBytes(i.ToString());
                    channel.BasicPublish(string.Empty, queueName, null, body);
                    channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
                }

                timer.Stop();
                System.Console.WriteLine($"Published {MESSAGE_COUNT:N0} messages individually in {timer.ElapsedMilliseconds:N0} ms");
            }
        }

        private static void PublishMessagesInBatch()
        {
            using (var connection = CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Declare a server-named queue
                var queueName = channel.QueueDeclare().QueueName;
                channel.ConfirmSelect();

                var batchSize = 100;
                var outstandingMessageCount = 0;

                var timer = new Stopwatch();
                timer.Start();

                for (int i = 0; i < MESSAGE_COUNT; i++)
                {
                    var body = Encoding.UTF8.GetBytes(i.ToString());
                    channel.BasicPublish(string.Empty, queueName, null, body);
                    outstandingMessageCount++;

                    if (outstandingMessageCount == batchSize)
                    {
                        channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
                        outstandingMessageCount = 0;
                    }
                }

                if (outstandingMessageCount > 0)
                    channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));

                timer.Stop();
                System.Console.WriteLine($"Published {MESSAGE_COUNT:N0} messages in batch in {timer.ElapsedMilliseconds:N0} ms");
            }
        }

        private static void HandlePublishConfirmsAsynchronously()
        {
            using (var connection = CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Declare a server-named queue
                var queueName = channel.QueueDeclare().QueueName;
                channel.ConfirmSelect();

                var outstandingConfirms = new ConcurrentDictionary<ulong, string>();

                void cleanOutstandingConfirms(ulong sequenceNumber, bool multiple)
                {
                    if (multiple)
                    {
                        var confirmed = outstandingConfirms.Where(k => k.Key <= sequenceNumber);
                        foreach (var entry in confirmed)
                            outstandingConfirms.TryRemove(entry.Key, out _);
                    }
                    else
                        outstandingConfirms.TryRemove(sequenceNumber, out _);
                }

                channel.BasicAcks += (sender, ea) => cleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);

                channel.BasicNacks += (sender, ea) =>
                {
                    outstandingConfirms.TryGetValue(ea.DeliveryTag, out string body);
                    System.Console.WriteLine($"Message with body '{body}' has been nack-ed. Sequence number: {ea.DeliveryTag}, multiple: {ea.Multiple}");
                    cleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
                };

                var timer = new Stopwatch();
                timer.Start();

                for (int i = 0; i < MESSAGE_COUNT; i++)
                {
                    outstandingConfirms.TryAdd(channel.NextPublishSeqNo, i.ToString());

                    var body = Encoding.UTF8.GetBytes(i.ToString());
                    channel.BasicPublish(string.Empty, queueName, null, body);
                }

                if (!WaitUntil(60, () => outstandingConfirms.IsEmpty))
                    throw new Exception("All messages could not be confirmed in 60 seconds");

                timer.Stop();
                System.Console.WriteLine($"Published {MESSAGE_COUNT:N0} messages and handled confirm asynchronously in {timer.ElapsedMilliseconds:N0} ms");
            }
        }

        private static bool WaitUntil(int numberOfSeconds, Func<bool> condition)
        {
            int waited = 0;

            while (!condition() && waited < numberOfSeconds * 1000)
            {
                Thread.Sleep(100);
                waited += 100;
            }

            return condition();
        }
    }
}

﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tutorial.RabbitMQ.Console.RPCClient
{
    class RPCClient
    {
        private const string QUEUE_NAME = "rpc_queue";

        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new ConcurrentDictionary<string, TaskCompletionSource<string>>();

        public RPCClient()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            // Declare a server-named queue.
            replyQueueName = channel.QueueDeclare().QueueName;
            
            consumer = new EventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;
        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            if (!callbackMapper.TryRemove(e.BasicProperties.CorrelationId, out TaskCompletionSource<string> tcs))
                return;

            var body = e.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            tcs.TrySetResult(response);
        }

        public Task<string> CallAsync(string message, CancellationToken cancellationToken = default(CancellationToken))
        {
            IBasicProperties props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            var messageBytes = Encoding.UTF8.GetBytes(message);
            var tcs = new TaskCompletionSource<string>();
            callbackMapper.TryAdd(correlationId, tcs);

            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: QUEUE_NAME, 
                                 basicProperties: props,
                                 body: messageBytes);

            channel.BasicConsume(consumer: consumer,
                                 queue: replyQueueName,
                                 autoAck: true);

            cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
            return tcs.Task;
        }

        public void Close()
        {
            connection.Close();
        }
    }

    public class Rpc
    {
       public  static void Main(string[] args)
        {
            System.Console.WriteLine($"{DateTime.Now}: RPC Client.");
            string n = args.Length > 0 ? args[0] : "30";
            Task t = InvokeAsync(n);
            t.Wait();

            System.Console.WriteLine($"{DateTime.Now}: Press [enter] to exit.");
            System.Console.ReadLine();
        }

        private static async Task InvokeAsync(string n)
        {
            var rpcClient = new RPCClient();

            System.Console.WriteLine($"{DateTime.Now}: Requesting fib({n}).");
            var response = await rpcClient.CallAsync(n.ToString());
            System.Console.WriteLine($"{DateTime.Now}: Got '{response}'");

            rpcClient.Close();
        }
    }
}

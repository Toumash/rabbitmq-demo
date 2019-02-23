using Microsoft.AspNetCore.SignalR.Client;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace rabbitmq_test_consumer
{
    class Program
    {
        public static volatile int connections = 0;
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "logs", type: "fanout");

                channel.QueueBind(queue: "test",
                                  exchange: "logs",
                                  routingKey: "");

                Console.WriteLine(" [*] Waiting for logs.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (s, e) =>
                {
                    connections++;
                    var body = e.Body;
                    var message = Encoding.UTF8.GetString(body);
                    channel.BasicAck(e.DeliveryTag, false);
                    Console.WriteLine("{1} {0}", message, DateTime.Now.ToString("hh.mm.ss.ffffff"));
                    UseSignalR(conn =>
                    {
                        Console.WriteLine("Invoking SendMessage please wait...");
                        conn.InvokeAsync("SendMessage", "toumash", "rabbitmq says hello :)" + DateTime.Now)
                        .ContinueWith(t =>
                        {
                            if (t.IsFaulted)
                            {
                                Console.Write("error");
                            }
                            else
                            {
                                Console.WriteLine("Sent to SignalR!!!");
                            }
                        }).Wait();
                    });
                };
                channel.BasicConsume(queue: "test",
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        public static void UseSignalR(Action<HubConnection> a)
        {
            Console.WriteLine("Creating signalr connection..");
            var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:5001/demohub/")
    .Build();
            Console.WriteLine("connection established");
            connection.StartAsync().ContinueWith(c => a(connection)).Wait();

        }
    }
}

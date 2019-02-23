using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading;

namespace rabbitmq_test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "logs", type: "fanout");
                channel.QueueBind("test", "logs", "", null);
                while (true)
                {
                    var message = GetMessage(args);
                    var body = Encoding.UTF8.GetBytes(message);
                    var props = channel.CreateBasicProperties();
                    props.DeliveryMode = 2;

                    channel.BasicPublish(exchange: "logs",
                                         routingKey: "",
                                         basicProperties: props,
                                         body: body);
                    Console.WriteLine("Hello!" + DateTime.Now);
                    Thread.Sleep(2000);
                    // Console.WriteLine("{1} [x] Sent {0}", message, DateTime.Now.ToString("hh.mm.ss.ffffff"));
                }
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0)
                   ? string.Join(" ", args)
                   : "info: Hello World!");
        }
    }
}

using Dominio;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace GrpcServerProgram.Servidor
{
    public class LogHelper
    {
        public static void PublishLog(string category, string content)
        {
            Log log = new Log();

            log.Category = category;
            log.Content = content;

            SendLog(log);
        }

        public static void SendLog(Log log)
        {
            //1 - definimos un FACTORY para inicializar la conexion
            //2 - definir la connection
            //3 - definir el channel
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //4 - Declaramos la cola de mensajes
                channel.QueueDeclare(queue: "logs",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                string messsage = JsonSerializer.Serialize(log);
                var body = Encoding.UTF8.GetBytes(messsage);
                channel.BasicPublish(exchange: "",
                    routingKey: "logs",
                    basicProperties: null,
                    body: body);

                Console.WriteLine(" [x] Sent {0}", log.Content);
            }
        }
    }
}

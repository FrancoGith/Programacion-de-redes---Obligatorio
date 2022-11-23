using System;
using System.Text;
using System.Text.Json;
using Dominio;
using LogServerLogic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WebApiRabbitMQ.Service
{
    public class MQService
    {
        public MQService() {

            // Conexión con RabbitMQ local: 
            var factory = new ConnectionFactory() { HostName = "localhost" }; // Defino la conexion

             var connection = factory.CreateConnection();
             var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "logs", // en el canal, definimos la Queue de la conexion
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            //Defino el mecanismo de consumo
            var consumer = new EventingBasicConsumer(channel);
            //Defino el evento que sera invocado cuando llegue un mensaje 
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);
                Log newLog = JsonSerializer.Deserialize<Log>(message);

                var logic = new LogLogic();
                logic.AddLog(newLog);
            };

            //"PRENDO" el consumo de mensajes
            channel.BasicConsume(queue: "logs",
                autoAck: true,
                consumer: consumer);
        }
    }
}

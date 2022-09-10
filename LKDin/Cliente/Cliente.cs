using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Cliente
{
    class Cliente
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Iniciando Cliente");
            // 1- Se crea el Socket del cliente
            var socketCliente = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // 2- Definimos un Endpoint local del cliente
            var endpointCliente = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            // 3- Asociación entre el socket y el endpoint local
            socketCliente.Bind(endpointCliente);

            // 4- Definir un Endpoint remoto con la Ip y el puerto que tenga el servidor. Debo conocer los datos a los que me voy a conectar
            var endpointServidor = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 14000);

            // 5- Establezco conexión con el socket y el endpoint remoto (Servidor)
            socketCliente.Connect(endpointServidor);
            Console.WriteLine("Conexión establecida");
            Console.WriteLine("Escriba un meensaje para el Servidor");
            bool exit = false;
            while (!exit)
            {
                string mensaje = Console.ReadLine();
                if (mensaje.Equals("exit"))
                {
                    exit = true;
                }

                byte[] data = Encoding.UTF8.GetBytes(mensaje);
                int largoData = data.Length; //guarda el largo del mensaje
                Console.WriteLine(largoData);
                socketCliente.Send(BitConverter.GetBytes(largoData)); //Parte fija
                socketCliente.Send(data); //Parte variable - mensaje
            }
        }
    }
}

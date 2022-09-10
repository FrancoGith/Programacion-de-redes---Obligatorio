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
            var socketCliente = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endpointCliente = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0); //TODO: Cambiar la string por una variable
            socketCliente.Bind(endpointCliente);

            var endpointServidor = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 14000); //TODO: Cambiar la string por una variable

            socketCliente.Connect(endpointServidor);
            Console.WriteLine("Conexión establecida");
            Console.WriteLine("Escriba un meensaje para el Servidor");
            bool exit = false;
            while (!exit)
            {
                //6 opciones

                Console.WriteLine(@"Elija una opción:
                1 - Alta de usuario
                2 - Alta de perfil de trabajo
                3 - Asociar foto de perfil a trabajo
                4 - Consultar perfiles existentes
                5 - Consultar perfil específico
                6 - Mensajes
                0 - Salir y desconectarse");

                int opcion = Int32.Parse(Console.ReadLine());

                byte[] data = Encoding.UTF8.GetBytes(opcion.ToString());
                int largoData = data.Length; 
                //Console.WriteLine(largoData);
                socketCliente.Send(BitConverter.GetBytes(largoData)); 
                socketCliente.Send(data); 
            }
        }
    }
}

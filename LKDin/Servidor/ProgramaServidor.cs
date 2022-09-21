using Dominio;
using Protocolo;
using System;
using System.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Servidor
{
    class ProgramaServidor
    {
        static int cantidadClientes = 2;
        private static DatosServidor datosServidor = new() { ListaUsuarios = new() };
        
        static void Main(string[] args)
        {
            Console.WriteLine("Levantando Servidor");
            var socketServidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 14000);
            socketServidor.Bind(endpoint);

            socketServidor.Listen(100);

            int clientesConectados = 0;
            while (clientesConectados <= cantidadClientes)
            {
                var socketCliente = socketServidor.Accept();
                clientesConectados++;
                Console.WriteLine("Cliente conectado");
                Thread thread = new Thread(() => ManejarCliente(socketCliente))
                {
                    IsBackground = true
                };
                thread.Start();
            }
            Console.WriteLine("Servidor desconectado");

        }

        static void ManejarCliente(Socket socketCliente)
        {
            bool clienteConectado = true;
            ManejoSockets manejoDataSocket = new ManejoSockets(socketCliente);
            while (clienteConectado)
            {
                try
                {
                    byte[] largoParteFija = manejoDataSocket.Receive(Constantes.LargoParteFija);
                    string parteFija = Encoding.UTF8.GetString(largoParteFija);
                    byte[] data = manejoDataSocket.Receive(int.Parse(parteFija.Substring(3)));
                    string mensajeUsuario = Encoding.UTF8.GetString(data);

                    Console.WriteLine($"Cliente dice: {mensajeUsuario}");

                    int comando = ObtenerComando(parteFija);

                    switch (comando)
                    {
                        case 1:
                            AltaDeUsuario(manejoDataSocket, mensajeUsuario);
                            break;
                        case 2:
                            AltaDePerfilDeTrabajo(manejoDataSocket, mensajeUsuario);
                            break;
                        default:
                            break;
                    }
                }
                catch (SocketException e)
                {
                    clienteConectado = false;
                }
            }
            Console.WriteLine("Cliente desconectado");
        }

        

        static void AltaDeUsuario(ManejoSockets manejoDataSocket, string mensajeUsuario)
        {
            string[] datos = mensajeUsuario.Split("#");
            datosServidor.ListaUsuarios.Add(new Usuario() { Username = datos[0], Password = datos[1] });
        }

        private static void AltaDePerfilDeTrabajo(ManejoSockets manejoDataSocket, string mensajeUsuario)
        {
            throw new NotImplementedException();
        }
        
        private static void AsociarFotoDePerfilATrabajo(Socket socketCliente)
        {
            throw new NotImplementedException();
        }

        private static void ConsultarPerfilesExistentes(Socket socketCliente)
        {
            throw new NotImplementedException();
        }
        
        private static void ConsultarPerfilEspecifico(Socket socketCliente)
        {
            throw new NotImplementedException();
        }
        
        private static void Mensajes(Socket socketCliente)
        {
            throw new NotImplementedException();
        }

        private static int ObtenerComando(string mensajeUsuario)
        {
            return int.Parse(mensajeUsuario.Substring(0, Constantes.LargoCodigo));
        }
    }
}

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
using System.Linq;
using Dominio.Mensajes;

namespace Servidor
{
    class ProgramaServidor
    {
        static int cantidadClientes = 2;
        private static DatosServidor datosServidor = new() { ListaUsuarios = new(), ListaHistoriales = new() };
        
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
                        case 60:
                            DevolverListaUsuarios(manejoDataSocket);
                            break;
                        case 61:
                            DevolverHistorialChat(manejoDataSocket, mensajeUsuario);
                            break;
                        case 62:
                            Mensajes(manejoDataSocket, mensajeUsuario);
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
        

        private static void DevolverListaUsuarios(ManejoSockets manejoDataSocket)
        {
            string mensaje = "";

            foreach (Usuario Usuario in datosServidor.ListaUsuarios)
            {
                mensaje += Usuario.Username + "#";
            }

            byte[] encodingMensaje = Encoding.UTF8.GetBytes(mensaje);
            string parteFija = "60" + encodingMensaje.Length.ToString().PadLeft(Constantes.LargoLongitudMensaje, '0'); ;
            byte[] encodingParteFija = Encoding.UTF8.GetBytes(parteFija);

            try
            {
                manejoDataSocket.Send(encodingParteFija);
                manejoDataSocket.Send(encodingMensaje);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static void DevolverHistorialChat(ManejoSockets manejoDataSocket, string cuerpo)
        {
            string[] usuarios = cuerpo.Split('#');

            HistorialChat historialDevolver = datosServidor.ListaHistoriales.FirstOrDefault(x => x.usuarios.Equals((usuarios[0], usuarios[1])) || x.usuarios.Equals((usuarios[1], usuarios[0])));

            if (historialDevolver == null)
            {
                historialDevolver = new HistorialChat
                {
                    usuarios = (usuarios[0], usuarios[1])
                };

                datosServidor.ListaHistoriales.Add(historialDevolver);
            }

            string mensaje = "";

            foreach (string chat in historialDevolver.mensajes)
            {
                mensaje += chat + "#";
            }

            byte[] encodingMensaje = Encoding.UTF8.GetBytes(mensaje);
            string parteFija = "60" + encodingMensaje.Length.ToString().PadLeft(Constantes.LargoLongitudMensaje, '0'); ;
            byte[] encodingParteFija = Encoding.UTF8.GetBytes(parteFija);

            try
            {
                manejoDataSocket.Send(encodingParteFija);
                manejoDataSocket.Send(encodingMensaje);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static void Mensajes(ManejoSockets socketCliente, string mensaje)
        {
            //[emisor, receptor, texto del mensaje]
            string[] contenido = mensaje.Split('#');

            HistorialChat chatActivo = datosServidor.ListaHistoriales.FirstOrDefault(x => x.usuarios.Equals((contenido[0], contenido[1])) || x.usuarios.Equals((contenido[1], contenido[0])));

            chatActivo.mensajes.Append(contenido[0] + " dice: " + contenido[2]);
        }

        private static int ObtenerComando(string mensajeUsuario)
        {
            return int.Parse(mensajeUsuario.Substring(0, Constantes.LargoCodigo));
        }
    }
}

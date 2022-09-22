using Dominio;
using Protocolo;
using Protocolo.ManejoArchivos;
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
        static readonly SettingsManager settingsManager = new SettingsManager();
        private static DatosServidor datosServidor = new() { Usuarios = new() };
        
        static void Main(string[] args)
        {
            Console.WriteLine("Levantando Servidor");

            string serverIP = settingsManager.ReadSettings(ServerConfig.serverIPconfigKey);
            int serverPort = int.Parse(settingsManager.ReadSettings(ServerConfig.serverPortconfigKey));
            int serverListen = int.Parse(settingsManager.ReadSettings(ServerConfig.serverListenconfigKey));
            int cantidadClientes = int.Parse(settingsManager.ReadSettings(ServerConfig.serverClientsconfigKey));

            var socketServidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endpoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
            
            socketServidor.Bind(endpoint);
            socketServidor.Listen(serverListen);

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
                        case 3:
                            AsociarFotoDePerfilATrabajo(manejoDataSocket, socketCliente, mensajeUsuario);
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
            datosServidor.Usuarios.Add(new Usuario() { Username = datos[0], Password = datos[1] });
        }

        private static void AltaDePerfilDeTrabajo(ManejoSockets manejoDataSocket, string mensajeUsuario)
        {
            string[] datos = mensajeUsuario.Split(Constantes.CaracterSeparador);
            Usuario usuario;
            try
            {
                usuario = datosServidor.GetUsuario(datos[0]);
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            List<string> habilidades = new List<string>(datos[1].Split(Constantes.CaracterSeparadorListas));
            string descripcion = datos[2];
            // TODO FOTO
            datosServidor.PerfilesTrabajo.Add(new PerfilTrabajo() { Usuario = usuario, Habilidades = habilidades, Descripcion = descripcion /*TODO FOTO*/});
        }
        
        private static void AsociarFotoDePerfilATrabajo(ManejoSockets manejoDataSocket, Socket socketCliente, string nombreUsuario)
        {
            PerfilTrabajo perfilUsuario = datosServidor.GetPerfilTrabajo(nombreUsuario);
            ManejoComunArchivo manejo = new ManejoComunArchivo(socketCliente);
            try
            {
                perfilUsuario.Foto = manejo.RecibirArchivo();
            } catch (Exception e)
            {
                EnviarMensajeCliente(e.Message, manejoDataSocket);
                Console.WriteLine("Ocurrio un error al recibir un archivo");
                return;
            }
            EnviarMensajeCliente("El servidor recibio el archivo", manejoDataSocket);
            Console.WriteLine("Se ha recibido un archivo");
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

        private static void EnviarMensajeCliente(string mensaje, ManejoSockets manejoDataSocket)
        {
            byte[] mensajeServidor = Encoding.UTF8.GetBytes(mensaje);
            string e1 = mensajeServidor.Length.ToString().PadLeft(Constantes.LargoLongitudMensaje, '0');
            byte[] parteFija = Encoding.UTF8.GetBytes(e1);
            try
            { // TODO Refactor a un metodo
                manejoDataSocket.Send(parteFija);
                manejoDataSocket.Send(mensajeServidor);
            }
            catch (Exception e2)
            {
                Console.WriteLine(e2);
            }
        }
    }
}

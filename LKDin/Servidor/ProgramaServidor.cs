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
using System.Linq;
using Dominio.Mensajes;

namespace Servidor
{
    class ProgramaServidor
    {
        static readonly SettingsManager settingsManager = new SettingsManager();
        private static DatosServidor datosServidor = new() { Usuarios = new(), ListaHistoriales = new() };
        
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
                        case 10:
                            LogIn(manejoDataSocket, mensajeUsuario);
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

        private static void LogIn(ManejoSockets manejoDataSocket, string mensaje)
        {
            string[] datos = mensaje.Split(Constantes.CaracterSeparador);

            Usuario usuarioLogIn = datosServidor.Usuarios.FirstOrDefault(x => x.Username == datos[0]);

            string codigo = "130000";

            if (usuarioLogIn != null && usuarioLogIn.Password == datos[1])
            {
                codigo = "120000";
            }

            byte[] encodingParteFija = Encoding.UTF8.GetBytes(codigo);

            try
            {
                manejoDataSocket.Send(encodingParteFija);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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

            foreach (Usuario Usuario in datosServidor.Usuarios)
            {
                mensaje += Usuario.Username + Constantes.CaracterSeparadorListas;
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
            string[] usuarios = cuerpo.Split(Constantes.CaracterSeparadorListas);

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
                mensaje += chat + Constantes.CaracterSeparadorListas;
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
            string[] contenido = mensaje.Split(Constantes.CaracterSeparador);

            HistorialChat chatActivo = datosServidor.ListaHistoriales.FirstOrDefault(x => x.usuarios.Equals((contenido[0], contenido[1])) || x.usuarios.Equals((contenido[1], contenido[0])));

            chatActivo.mensajes.Add(contenido[0] + " dice: " + contenido[2]);
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

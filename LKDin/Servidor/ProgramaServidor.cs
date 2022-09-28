using Dominio;
using Protocolo;
using Protocolo.ManejoArchivos;
using System;
using System.Data;
using System.Linq;
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

        private static DatosServidor datosServidor = new DatosServidor();
        
        static void Main(string[] args)
        {

            List<string> hab = new() { "LoL", "Programacion" };
            datosServidor.PerfilesTrabajo.Add(new PerfilTrabajo() { Usuario = new() { Username = "Usuario 1" }, Descripcion = "Me falta pala", Habilidades = hab });
            datosServidor.PerfilesTrabajo.Add(new PerfilTrabajo() { Usuario = new() { Username = "Usuario 2" }, Descripcion = "Se muchas cosas", Habilidades = hab });
            datosServidor.PerfilesTrabajo.Add(new PerfilTrabajo() { Usuario = new() { Username = "Usuario 3" }, Descripcion = "Bases", Habilidades = hab });


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

                    Console.WriteLine($"[Cliente] {mensajeUsuario}");

                    int comando = ObtenerComando(parteFija);

                    switch (comando)
                    {
                        case 1:
                            LogIn(manejoDataSocket, mensajeUsuario);
                            break;
                        case 10:
                            AltaDeUsuario(manejoDataSocket, mensajeUsuario);
                            break;
                        case 20:
                            AltaDePerfilDeTrabajo(manejoDataSocket, mensajeUsuario);
                            break;
                        case 30:
                            AsociarFotoDePerfilATrabajo(manejoDataSocket, socketCliente, mensajeUsuario);
                            break;
                        case 40:
                            ConsultarPerfilesExistentes(manejoDataSocket, mensajeUsuario);
                            break;
                        case 50:
                            ConsultarPerfilEspecifico(manejoDataSocket, mensajeUsuario);
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
            string[] datos = mensajeUsuario.Split("ϴ");
            datosServidor.Usuarios.Add(new Usuario() { Username = datos[0], Password = datos[1] });
            
            EnviarMensajeCliente("Usuario creado", manejoDataSocket);
            Console.WriteLine("Se ha creado un nuevo usuario");
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
            datosServidor.PerfilesTrabajo.Add(new PerfilTrabajo() { Usuario = usuario, Habilidades = habilidades, Descripcion = descripcion});

            EnviarMensajeCliente("Perfil de trabajo creado", manejoDataSocket);
            Console.WriteLine("Se ha creado un nuevo perfil de trabajo");
        }
        
        private static void AsociarFotoDePerfilATrabajo(ManejoSockets manejoDataSocket, Socket socketCliente, string nombreUsuario)
        {
            PerfilTrabajo perfilUsuario;
            try
            {
                perfilUsuario = datosServidor.GetPerfilTrabajo(nombreUsuario);
                EnviarMensajeCliente("[Servidor] Usuario encontrado", manejoDataSocket);
            } catch(Exception e)
            {
                EnviarMensajeCliente(e.Message, manejoDataSocket);
                return;
            }
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

            string codigo = "030000";

            if (usuarioLogIn != null && usuarioLogIn.Password == datos[1])
            {
                codigo = "020000";
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

        private static void ConsultarPerfilesExistentes(ManejoSockets socketCliente, string mensajeUsuario)
        {
            string[] datos = mensajeUsuario.Split("ϴ");

            List<string> habilidades = datos[0].Split(" ").ToList();
            habilidades = habilidades.Select(habilidad => habilidad.ToUpper()).ToList();

            List<string> palabras = datos[1].Split(" ").ToList();
            palabras = palabras.Select(palabra => palabra.ToUpper()).ToList();

            List<string> usuariosEncontrados = new();

            foreach (var perfil in datosServidor.PerfilesTrabajo)
            {
                List<string> infoPerfil = perfil.GetSearchData().Split(" ").ToList();
                infoPerfil = infoPerfil.Select(info => info.ToUpper()).ToList();
                foreach (string info in infoPerfil)
                {
                    if (habilidades.Contains(info) || palabras.Contains(info))
                    {
                        if (!usuariosEncontrados.Contains(perfil.Usuario.Username))
                        {
                            usuariosEncontrados.Add(perfil.Usuario.Username);
                        }
                    }
                }
            }

            Console.WriteLine("Se termino la busqueda de usuarios");

            string respuestaUsuario = "";
            if (usuariosEncontrados.Count != 0)
            {
                respuestaUsuario = "Usuarios encontrados: ";
                foreach (string nombreUsuario in usuariosEncontrados)
                {
                    PerfilTrabajo perfilTrabajo = ObtenerPerfilTrabajo(nombreUsuario);
                    string habilidadesPerfil = string.Join("-", perfilTrabajo.Habilidades);
                    string resumenPerfil = $"\n    Nombre: {perfilTrabajo.Usuario.Username}\n    Descripción: {perfilTrabajo.Descripcion}\n    Habilidades: {habilidadesPerfil}\n";
                    respuestaUsuario = $"{respuestaUsuario} \n {resumenPerfil}";
                }
            }
            else
            {
                respuestaUsuario = "\nNo se encontraron coincidencias\n";
            }
            EnviarMensajeCliente(respuestaUsuario, socketCliente);
            Console.WriteLine("Se han buscado perfiles");
        }

        private static void ConsultarPerfilEspecifico(ManejoSockets socketCliente, string mensajeUsuario)
        {
            string[] datos = mensajeUsuario.Split("ϴ");

            PerfilTrabajo usuarioEncontrado = datosServidor.PerfilesTrabajo.FirstOrDefault(usuario => usuario.Usuario.Username == datos[0]);

            string respuestaUsuario = "";
            if (usuarioEncontrado != null)
            {
                string habilidades = string.Join("-", usuarioEncontrado.Habilidades);
                respuestaUsuario = $"\nUsuario encontrado\n    Nombre: {usuarioEncontrado.Usuario.Username}\n    Descripción: {usuarioEncontrado.Descripcion}\n    Habilidades: {habilidades}\n";
            }
            else
            {
                respuestaUsuario = "\nPerfil de trabajo no existente\n";
            }
            EnviarMensajeCliente(respuestaUsuario, socketCliente);
            Console.WriteLine("Se ha buscado un perfil especifico");
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

        private static PerfilTrabajo ObtenerPerfilTrabajo(string nombreUsuario)
        {
            return datosServidor.PerfilesTrabajo.FirstOrDefault(perfil => perfil.Usuario.Username == nombreUsuario);
        }
        
        private static void EnviarMensajeCliente(string mensaje, ManejoSockets manejoDataSocket)
        {
            byte[] mensajeServidor = Encoding.UTF8.GetBytes(mensaje);
            string e1 = mensajeServidor.Length.ToString().PadLeft(Constantes.LargoLongitudMensaje, '0');
            string e2 = "00" + e1;
            byte[] parteFija = Encoding.UTF8.GetBytes(e2);
            try
            {
                manejoDataSocket.Send(parteFija);
                manejoDataSocket.Send(mensajeServidor);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}

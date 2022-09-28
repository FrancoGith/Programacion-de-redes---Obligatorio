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
using System.Security.Policy;

namespace Servidor
{
    class ProgramaServidor
    {
        static readonly SettingsManager settingsManager = new SettingsManager();

        private static DatosServidor datosServidor = new DatosServidor();
        
        static void Main(string[] args)
        {
            /*datosServidor.AgregarUsuario("U1", "U1");
            datosServidor.AgregarUsuario("U2", "U2");
            datosServidor.AgregarUsuario("U3", "U3");

            datosServidor.AgregarPerfilTrabajo(datosServidor.GetUsuario("U1"), new List<string>() { "C#", "Java" }, "Programador");
            datosServidor.AgregarPerfilTrabajo(datosServidor.GetUsuario("U2"), new List<string>() { "Python" }, "Backend");
            datosServidor.AgregarPerfilTrabajo(datosServidor.GetUsuario("U3"), new List<string>() { "Java" }, "QA");*/

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
                        case 51:
                            EnviarImagenPerfilEspecifico(manejoDataSocket, socketCliente, mensajeUsuario);
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

            if (datosServidor.GetUsuario(datos[0]) != null)
            {
                EnviarMensajeCliente("Usuario existente, intente con otro nombre.", manejoDataSocket);
                Console.WriteLine("No se ha ingresado el usuario porque ya existia");
            }
            else
            {
                datosServidor.AgregarUsuario(datos[0], datos[1]);
                EnviarMensajeCliente("Usuario creado", manejoDataSocket);
                Console.WriteLine("Se ha creado un nuevo usuario");
            }
        }

        private static void AltaDePerfilDeTrabajo(ManejoSockets manejoDataSocket, string mensajeUsuario)
        {
            // TODO: que exista el usuario
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

            datosServidor.AgregarPerfilTrabajo(usuario, habilidades, descripcion);

            EnviarMensajeCliente("Perfil de trabajo creado", manejoDataSocket);
            Console.WriteLine("Se ha creado un nuevo perfil de trabajo");
        }

        private static void AsociarFotoDePerfilATrabajo(ManejoSockets manejoDataSocket, Socket socketCliente, string nombreUsuario)
        {
            // TODO: extraer foto a datosServidor.
            PerfilTrabajo perfilUsuario = datosServidor.GetPerfilTrabajo(nombreUsuario);
            string codigo = "310000";
            
            if (perfilUsuario == null) {
                codigo = "320000";
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

            if (codigo == "310000")
            {
                // seba
                ManejoComunArchivo manejo = new ManejoComunArchivo(socketCliente);
                string nombreArchivo = $"imagenes\\foto{nombreUsuario}";
                try
                {
                    perfilUsuario.Foto = manejo.RecibirArchivo(nombreArchivo);
                } catch (Exception e)
                {
                    EnviarMensajeCliente(e.Message, manejoDataSocket);
                    Console.WriteLine("Ocurrio un error al recibir un archivo");
                    return;
                }
                EnviarMensajeCliente("El servidor recibio el archivo", manejoDataSocket);
                Console.WriteLine("Se ha recibido un archivo");
            }
            else
            {
                Console.WriteLine("El usuario ingresado por el cliente no existe");
            }
        }

        private static void LogIn(ManejoSockets manejoDataSocket, string mensaje)
        {
            string[] datos = mensaje.Split(Constantes.CaracterSeparador);

            Usuario usuarioLogIn = datosServidor.GetUsuario(datos[0]);

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

            foreach (var perfil in datosServidor.GetPerfilesTrabajo())
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
                    PerfilTrabajo perfilTrabajo = datosServidor.GetPerfilTrabajo(nombreUsuario);
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
            string[] datos = mensajeUsuario.Split(Constantes.CaracterSeparador);

            PerfilTrabajo usuarioEncontrado = datosServidor.GetPerfilTrabajo(datos[0]);

            string respuestaUsuario = "";
            if (usuarioEncontrado != null)
            {
                string habilidades = string.Join("-", usuarioEncontrado.Habilidades);
                respuestaUsuario = $"\nUsuario encontrado\n    Nombre: {usuarioEncontrado.Usuario.Username}\n    Descripción: {usuarioEncontrado.Descripcion}\n    Habilidades: {habilidades}\n    Imagen: {usuarioEncontrado.Foto}\n";
            }
            else
            {
                respuestaUsuario = "\nPerfil de trabajo no existente\n";
            }
            EnviarMensajeCliente(respuestaUsuario, socketCliente);
            Console.WriteLine("Se ha buscado un perfil especifico");

        }

        private static void EnviarImagenPerfilEspecifico(ManejoSockets manejoDataSocket,Socket socketCliente, string nombreUsuario)
        {
            PerfilTrabajo perfil;
            try
            {
                perfil = datosServidor.GetPerfilTrabajo(nombreUsuario);
            } catch(Exception e)
            {
                EnviarMensajeCliente("Perfil de trabajo no existente", manejoDataSocket);
                Console.WriteLine(e.Message);
                return;
            }
            if (perfil.Foto != String.Empty)
            {
                string pathApp = Directory.GetCurrentDirectory();
                string absPath = Path.Combine(pathApp, perfil.Foto);
                string nombreArchivo = "imagenes\\" + Path.GetFileNameWithoutExtension(perfil.Foto);
                EnviarMensajeCliente("Ok" + Constantes.CaracterSeparador + nombreArchivo, manejoDataSocket);
                ManejoComunArchivo fileCommonHandler = new ManejoComunArchivo(socketCliente);
                fileCommonHandler.SendFile(absPath);
            } else
            {
                EnviarMensajeCliente("Este perfil de trabajo no tiene ninguna foto asociada", manejoDataSocket);
            }
        }
        
        private static void DevolverListaUsuarios(ManejoSockets manejoDataSocket)
        {
            string mensaje = "";

            foreach (Usuario Usuario in datosServidor.GetUsuarios())
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

            HistorialChat historialDevolver = datosServidor.GetHistorial(usuarios);

            if (historialDevolver == null)
            {
                historialDevolver = new HistorialChat
                {
                    usuarios = (usuarios[0], usuarios[1])
                };

                datosServidor.AgregarHistorial(historialDevolver);
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

            HistorialChat chatActivo = datosServidor.GetHistorial(contenido);

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

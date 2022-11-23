using Dominio;
using Dominio.Mensajes;
using GrpcServerProgram.Servidor;
using Protocolo;
using Protocolo.ManejoArchivos;
using System.Data;
using System.Net;
using System.Net.Sockets;

namespace Servidor
{
    class ProgramaServidor
    {
        static readonly SettingsManager settingsManager = new SettingsManager();
        private static DatosServidor datosServidor = new DatosServidor();
        private readonly TcpListener tcpListener;
        private bool aceptandoConexiones;

        string serverIP = settingsManager.ReadSettings(ServerConfig.serverIPconfigKey);
        int serverPort = int.Parse(settingsManager.ReadSettings(ServerConfig.serverPortconfigKey));
        int serverListen = int.Parse(settingsManager.ReadSettings(ServerConfig.serverListenconfigKey));
        int cantidadClientes = int.Parse(settingsManager.ReadSettings(ServerConfig.serverClientsconfigKey));


        public ProgramaServidor()
        {
            datosServidor.AgregarUsuario("U1", "U1");
            datosServidor.AgregarUsuario("U2", "U2");
            datosServidor.AgregarUsuario("U3", "U3");

            datosServidor.AgregarPerfilTrabajo(datosServidor.GetUsuario("U1"), new List<string>() { "C#", "Java" }, "Programador");
            datosServidor.AgregarPerfilTrabajo(datosServidor.GetUsuario("U2"), new List<string>() { "Python" }, "Backend");
            datosServidor.AgregarPerfilTrabajo(datosServidor.GetUsuario("U3"), new List<string>() { "Java" }, "QA");

            var endpoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
            tcpListener = new TcpListener(endpoint);
            aceptandoConexiones = true;

            Console.WriteLine($"Servidor en la dirección {serverIP} y puerto {serverPort}");
        }

        public async Task ComenzarRecibirConexiones()
        {
            tcpListener.Start(cantidadClientes);
            while (aceptandoConexiones)
            {
                try
                {
                    var tcpClientSocket = await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
                    Task task = Task.Run(async () => await ManejarCliente(tcpClientSocket).ConfigureAwait(false));
                }
                catch (SocketException)
                {
                    Console.WriteLine("No se aceptan peticiones");
                }
            }
        }

        static async Task ManejarCliente(TcpClient tcpClient)
        {
            bool clienteConectado = true;
            using (var stream = tcpClient.GetStream())
            {
                ManejoStreamsHelper manejoDataSocket = new ManejoStreamsHelper(stream);
                while (clienteConectado)
                {
                    try
                    {
                        string parteFija = await manejoDataSocket.Recieve();
                        string mensajeUsuario = await manejoDataSocket.Recieve();

                        Console.WriteLine($"[Cliente] {mensajeUsuario}");

                        int comando = ObtenerComando(parteFija);

                        switch (comando)
                        {
                            case 1:
                                await LogIn(manejoDataSocket, mensajeUsuario);
                                break;
                            case 10:
                                await AltaDeUsuario(manejoDataSocket, mensajeUsuario);
                                break;
                            case 20:
                                await AltaDePerfilDeTrabajo(manejoDataSocket, mensajeUsuario);
                                break;
                            case 30:
                                await AsociarFotoDePerfilATrabajo(manejoDataSocket, mensajeUsuario);
                                break;
                            case 40:
                                await ConsultarPerfilesExistentes(manejoDataSocket, mensajeUsuario);
                                break;
                            case 50:
                                await ConsultarPerfilEspecifico(manejoDataSocket, mensajeUsuario);
                                break;
                            case 51:
                                await EnviarImagenPerfilEspecifico(manejoDataSocket, mensajeUsuario);
                                break;
                            case 60:
                                await DevolverListaUsuarios(manejoDataSocket);
                                break;
                            case 61:
                                await DevolverHistorialChat(manejoDataSocket, mensajeUsuario);
                                break;
                            case 62:
                                Mensajes(manejoDataSocket, mensajeUsuario);
                                break;
                            case 63:
                                await DevolverListaNoLeidos(manejoDataSocket, mensajeUsuario);
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
            }
            Console.WriteLine("Cliente desconectado");
        }

        static async Task AltaDeUsuario(ManejoStreamsHelper manejoDataSocket, string mensajeUsuario)
        {
            string[] datos = mensajeUsuario.Split(Constantes.CaracterSeparador);
            string log = string.Empty;

            if (datosServidor.GetUsuario(datos[0]) != null)
            {
                await EnviarMensajeCliente(manejoDataSocket, "Usuario existente, intente con otro nombre.", "12");
                log = "No se ha ingresado el usuario porque ya existia";
                Console.WriteLine(log);
                
            }
            else
            {
                datosServidor.AgregarUsuario(datos[0], datos[1]);
                await EnviarMensajeCliente(manejoDataSocket, "Usuario creado", "11");
                log = "Se ha creado un nuevo usuario";
                Console.WriteLine(log);
            }

            LogHelper.PublishLog(datos[0], "Creación usuario", log);
        }

        private static async Task AltaDePerfilDeTrabajo(ManejoStreamsHelper manejoDataSocket, string mensajeUsuario)
        {
            string[] datos = mensajeUsuario.Split(Constantes.CaracterSeparador);

            try
            {
                Usuario usuario = datosServidor.GetUsuario(datos[0]);
                PerfilTrabajo perfilTrabajo = datosServidor.GetPerfilTrabajo(datos[0]);
                string log = string.Empty;

                if (usuario != null)
                {
                    if (perfilTrabajo != null)
                    {
                        await EnviarMensajeCliente(manejoDataSocket, "Perfil de trabajo existente para este usuario", "22");
                        log = "Perfil de trabajo existente para este usuario";
                        Console.WriteLine(log);
                    }
                    else
                    {
                        List<string> habilidades = new List<string>(datos[1].Split(Constantes.CaracterSeparadorListas));
                        string descripcion = datos[2];

                        datosServidor.AgregarPerfilTrabajo(usuario, habilidades, descripcion);

                        await EnviarMensajeCliente(manejoDataSocket, "Perfil de trabajo creado", "23");
                        log = "Perfil de trabajo existente para este usuario";
                        Console.WriteLine(log);
                    }
                    LogHelper.PublishLog(datos[0], "Creación perfil trabajo", log);
                }
                else
                {
                    await EnviarMensajeCliente(manejoDataSocket, "Usuario inexistente para crear perfil de trabajo", "21");
                    log = "Usuario inexistente para crear perfil de trabajo";
                    Console.WriteLine(log);
                    LogHelper.PublishLog("Usuario inexistente", "Creación perfil trabajo", log);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }

        private async static Task AsociarFotoDePerfilATrabajo(ManejoStreamsHelper manejoDataSocket, string nombreUsuario)
        {
            PerfilTrabajo perfilUsuario = datosServidor.GetPerfilTrabajo(nombreUsuario);
            string codigo = "31";
            string mensaje = "Usuario existente";
            string log = string.Empty;

            if (perfilUsuario == null)
            {
                codigo = "32";
                mensaje = "Usuario no existente";
            }

            await EnviarMensajeCliente(manejoDataSocket, mensaje, codigo);

            if (codigo == "31")
            {
                ManejoComunArchivo manejo = new ManejoComunArchivo(manejoDataSocket.stream.Socket);
                string nombreArchivo = $"imagenes\\foto{nombreUsuario}";
                string pathApp = Directory.GetCurrentDirectory();
                string absPath = Path.Combine(pathApp, perfilUsuario.Foto);
                if (VerificacionExistenciaArchivos.FileExists(absPath))
                {
                    File.Delete(perfilUsuario.Foto);
                }
                try
                {
                    perfilUsuario.Foto = await manejo.RecieveFile(nombreArchivo);
                }
                catch (Exception e)
                {
                    await EnviarMensajeCliente(manejoDataSocket, e.Message, "00");
                    log = "Ocurrió un error al recibir un archivo";
                    Console.WriteLine(log);
                    LogHelper.PublishLog(nombreUsuario, "Asociación foto perfil", log);
                    return;
                }
                await EnviarMensajeCliente(manejoDataSocket, "El servidor recibio el archivo", "33");
                log = "Se ha recibido un archivo";
                Console.WriteLine(log);
                LogHelper.PublishLog(nombreUsuario, "Asociación foto perfil", log);
            }
            else
            {
                log = "El usuario ingresado por el cliente no existe";
                Console.WriteLine(log);
                LogHelper.PublishLog("Usuario inexistente", "Asociación foto perfil", log);
            }
        }

        private static async Task LogIn(ManejoStreamsHelper manejoDataSocket, string mensaje)
        {
            string[] datos = mensaje.Split(Constantes.CaracterSeparador);

            Usuario usuarioLogIn = datosServidor.GetUsuario(datos[0]);

            string codigo = "030000";
            string log = "LogIn rechazado";

            if (usuarioLogIn != null && usuarioLogIn.Password == datos[1])
            {
                codigo = "020000";
                log = "LogIn exitoso";
            }

            try
            {
                await manejoDataSocket.Send(codigo);
                await manejoDataSocket.Send("");
                LogHelper.PublishLog(datos[0], "LogIn", log);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception thrown - Message: {e.Message}");
                throw;
            }
        }

        private static async Task ConsultarPerfilesExistentes(ManejoStreamsHelper socketCliente, string mensajeUsuario)
        {
            string[] datos = mensajeUsuario.Split(Constantes.CaracterSeparador);

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
            await EnviarMensajeCliente(socketCliente, respuestaUsuario, "00");
            Console.WriteLine("Se han buscado perfiles");
        }

        private static async Task ConsultarPerfilEspecifico(ManejoStreamsHelper socketCliente, string mensajeUsuario)
        {
            string[] datos = mensajeUsuario.Split(Constantes.CaracterSeparador);

            PerfilTrabajo usuarioEncontrado = datosServidor.GetPerfilTrabajo(datos[0]);

            string respuestaUsuario = "";
            if (usuarioEncontrado != null)
            {
                string habilidades = string.Join("-", usuarioEncontrado.Habilidades);
                respuestaUsuario = $"\nUsuario encontrado\n    Nombre: {usuarioEncontrado.Usuario.Username}\n    Descripción: {usuarioEncontrado.Descripcion}\n    Habilidades: {habilidades}\n    Imagen: {usuarioEncontrado.Foto}\n" + Constantes.CaracterSeparador + $"{usuarioEncontrado.Foto}";
            }
            else
            {
                respuestaUsuario = "\nPerfil de trabajo no existente\n";
            }
            await EnviarMensajeCliente(socketCliente, respuestaUsuario, "98");
            Console.WriteLine("Se ha buscado un perfil especifico");

        }

        private static async Task EnviarImagenPerfilEspecifico(ManejoStreamsHelper manejoDataSocket, string nombreUsuario)
        {
            PerfilTrabajo perfil;
            try
            {
                perfil = datosServidor.GetPerfilTrabajo(nombreUsuario);
            }
            catch (Exception e)
            {
                await EnviarMensajeCliente(manejoDataSocket, "Perfil de trabajo no existente", "53");
                Console.WriteLine(e.Message);
                return;
            }

            if (perfil.Foto != String.Empty)
            {
                string pathApp = Directory.GetCurrentDirectory();
                string absPath = Path.Combine(pathApp, perfil.Foto);
                string nombreArchivo = "imagenes\\" + Path.GetFileNameWithoutExtension(perfil.Foto);
                await EnviarMensajeCliente(manejoDataSocket, "Ok" + Constantes.CaracterSeparador + nombreArchivo, "52");
                ManejoComunArchivo fileCommonHandler = new ManejoComunArchivo(manejoDataSocket.stream.Socket);
                await fileCommonHandler.SendFile(absPath);
            }
            else
            {
                await EnviarMensajeCliente(manejoDataSocket, "Este perfil de trabajo no tiene ninguna foto asociada", "54");
            }
        }

        private static async Task DevolverListaUsuarios(ManejoStreamsHelper manejoDataSocket)
        {
            string mensaje = "";

            foreach (Usuario Usuario in datosServidor.GetUsuarios())
            {
                mensaje += Usuario.Username + Constantes.CaracterSeparadorListas;
            }

            await EnviarMensajeCliente(manejoDataSocket, mensaje, "60");
        }

        private static async Task DevolverListaNoLeidos(ManejoStreamsHelper manejoDataSocket, string nombreUsuario)
        {
            string mensaje = string.Empty;

            foreach (Usuario Usuario in datosServidor.GetUsuarios())
            {
                mensaje += Usuario.Username;
                mensaje += Constantes.CaracterSeparador;

                string[] nombres = { nombreUsuario, Usuario.Username };
                HistorialChat historial = datosServidor.GetHistorial(nombres);

                if (historial != null && historial.ultimoEnHablar != nombreUsuario && historial.visto == false)
                {
                    mensaje += 1;
                }
                else
                {
                    mensaje += 0;
                }

                mensaje += Constantes.CaracterSeparadorListas;
            }

            await EnviarMensajeCliente(manejoDataSocket, mensaje, "63");
        }

        private static async Task DevolverHistorialChat(ManejoStreamsHelper manejoDataSocket, string cuerpo)
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
            //Esto es para marcar como leido
            else if (usuarios[1].Equals(historialDevolver.ultimoEnHablar))
            {
                historialDevolver.visto = true;
            }

            string mensaje = "";

            foreach (string chat in historialDevolver.mensajes)
            {
                mensaje += chat + Constantes.CaracterSeparadorListas;
            }

            await EnviarMensajeCliente(manejoDataSocket, mensaje, "60");
        }

        private static void Mensajes(ManejoStreamsHelper socketCliente, string mensaje)
        {
            //[emisor, receptor, texto del mensaje]
            string[] contenido = mensaje.Split(Constantes.CaracterSeparador);

            HistorialChat chatActivo = datosServidor.GetHistorial(contenido);

            chatActivo.ultimoEnHablar = contenido[0];
            chatActivo.visto = false;
            chatActivo.mensajes.Add(contenido[0] + " dice: " + contenido[2]);
        }

        private static int ObtenerComando(string mensajeUsuario)
        {
            return int.Parse(mensajeUsuario.Substring(0, Constantes.LargoCodigo));
        }

        private static async Task EnviarMensajeCliente(ManejoStreamsHelper manejoDataSocket, string mensaje, string code)
        {
            try
            {
                await manejoDataSocket.Send(code);
                await manejoDataSocket.Send(mensaje);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception thrown - Message: {e.Message}");
            }
        }
    }
}

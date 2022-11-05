using Dominio;
using Protocolo;
using Protocolo.ManejoArchivos;
using Servidor;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Cliente
{
    class Cliente
    {
        static readonly SettingsManager settingsManager = new SettingsManager();

        static async Task Main(string[] args)
        {
            //Esto es porque las funciones son estáticas
            string username = "";

            Console.WriteLine("Iniciando Cliente");

            string endPointClienteserverIp = settingsManager.ReadSettings(ClientConfig.endPointClienteIPconfigKey);
            int endPointClienteserverPort = int.Parse(settingsManager.ReadSettings(ClientConfig.endPointClientePortconfigKey));

            string serverIp = settingsManager.ReadSettings(ClientConfig.serverIPconfigKey);
            int serverPort = int.Parse(settingsManager.ReadSettings(ClientConfig.serverPortconfigKey));

            var endpointCliente = new IPEndPoint(IPAddress.Parse(endPointClienteserverIp), endPointClienteserverPort);
            var endpointServidor = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);

            var tcpClient = new TcpClient(endpointCliente);

            await tcpClient.ConnectAsync(endpointServidor);

            Console.WriteLine("Conexión establecida");

            bool login = false;

            using (var stream = tcpClient.GetStream())
            {
                ManejoStreamsHelper manejoStreams = new ManejoStreamsHelper(stream);
                while (!login)
                {
                    Console.WriteLine(@"Elija una opción:
                    1 - Iniciar sesión
                    2 - Crear una cuenta");
                    try
                    {
                        int eleccion = int.Parse(Console.ReadLine());
                        (bool, string) verificacion;

                        switch (eleccion)
                        {
                            case 1:
                                verificacion = await VerificarCredenciales(manejoStreams);

                                login = verificacion.Item1;
                                username = verificacion.Item2;
                                break;
                            case 2:
                                verificacion = await AltaUsuario(manejoStreams);
                                login = verificacion.Item1;
                                username = verificacion.Item2;
                                break;
                            default:
                                Console.WriteLine("Ingrese una opción válida");
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Ingrese una opción válida");
                    }
                }

                Console.WriteLine("Escriba un mensaje para el Servidor");
                bool exit = false;
                while (!exit)
                {
                    Console.WriteLine(@"Elija una opción:
                1 - Alta de usuario
                2 - Alta de perfil de trabajo
                3 - Asociar foto de perfil a trabajo
                4 - Consultar perfiles existentes
                5 - Consultar perfil específico
                6 - Mensajes
                0 - Salir y desconectarse");
                    int opcion = 9999;
                    try
                    {
                        opcion = int.Parse(Console.ReadLine());
                        switch (opcion)
                        {
                            case 1:
                                await AltaUsuario(manejoStreams);
                                break;
                            case 2:
                                await AltaDePerfilDeTrabajo(manejoStreams);
                                break;
                            case 3:
                                await AsociarFotoDePerfilATrabajo(manejoStreams);
                                break;
                            case 4:
                                await ConsultarPerfilesExistentes(manejoStreams);
                                break;
                            case 5:
                                await ConsultarPerfilEspecifico(manejoStreams);
                                break;
                            case 6:
                                await Mensajes(manejoStreams, username);
                                break;
                            case 0:
                                exit = true;
                                Desconexion(tcpClient);
                                break;
                            default:
                                Console.WriteLine("Ingrese una opción válida");
                                break;
                        }
                    }
                    catch (FormatException) { opcion = 9999; }
                    catch (ArgumentNullException) { opcion = 9999; }
                    catch (OverflowException) { opcion = 9999; }

                }

            }
        }

        private async static Task<(bool,string)> VerificarCredenciales(ManejoStreamsHelper manejoDataSocket)
        {
            Console.Clear();
            Console.WriteLine("Log in\n");

            string username;
            do
            {
                Console.WriteLine("Escriba el nombre de usuario");
                username = Console.ReadLine().Trim();

                if (string.IsNullOrWhiteSpace(username))
                {
                    Console.WriteLine("El nombre de usuario no puede estar vacío");
                }
            }
            while (string.IsNullOrWhiteSpace(username));

            string password;
            do
            {
                Console.WriteLine("Escriba la contraseña");
                password = Console.ReadLine().Trim();
                if (string.IsNullOrWhiteSpace(password))
                {
                    Console.WriteLine("La contraseña no puede estar vacía");
                }
            }
            while (string.IsNullOrWhiteSpace(password));

            //Mando informacion de login

            string mensaje = username + Constantes.CaracterSeparador + password;

            string respuesta = await ComunicacionServidorCliente(manejoDataSocket, mensaje, "01");

            respuesta = respuesta.Substring(0, 2);

            if (int.Parse(respuesta) == 2)
            {
                Console.WriteLine("Log in realizado con éxito");
                return (true,username);
            }
            else if (int.Parse(respuesta) == 3)
            {
                Console.WriteLine("Error: nombre de usuario o contraseña incorrectos");
                return (false,"");
            }
            else
            {
                Console.WriteLine("Error desconocido"); //Esto no se debería ejecutar nunca pero lo pongo para que c# no se queje
                return (false,"");
            }
        }

        private async static Task<(bool, string)> AltaUsuario(ManejoStreamsHelper manejoDataSocket)
        {
            Console.Clear();
            Console.WriteLine("Alta de usuario\n");

            Console.WriteLine("Escriba el nombre de usuario");
            string username = Console.ReadLine().Trim();
            Console.WriteLine("Escriba la contraseña");
            string password = Console.ReadLine().Trim();
            
            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("El nombre de usuario no puede estar vacío");
                return (false, "");
            }
            else if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("La contraseña no puede estar vacía");
                return (false, "");
            }

            string mensaje = username + Constantes.CaracterSeparador + password;
            string respuesta = await ComunicacionServidorCliente(manejoDataSocket, mensaje, "10");
            respuesta = respuesta.Substring(0, 2);

            if (int.Parse(respuesta) == 12)
            {
                Console.WriteLine("Error: usuario existente");
                return (false, "");
            }
            else if (int.Parse(respuesta) == 11)
            {
                Console.WriteLine("Creacion de usuario realizada con éxito");
                return (true, username);
            }
            else
            {
                Console.WriteLine("Error desconocido"); //Esto no se debería ejecutar nunca pero lo pongo para que c# no se queje
                return (false, "");
            }
        }

        private async static Task AltaDePerfilDeTrabajo(ManejoStreamsHelper manejoDataSocket)
        {
            Console.WriteLine("Alta Perfil de Trabajo");
            Console.WriteLine("Ingrese el nombre del usuario del perfil a crear:");
            string username = Console.ReadLine().Trim();
            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("El nombre de usuario no puede estar vacío");
                return;
            }
            
            List<string> habilidades = new List<string>();
            Console.WriteLine(@"Ingrese las habilidades una por una: (código de escape: 'quit')");
            string habilidad = Console.ReadLine().Trim();
            while (habilidad != "quit")
            {
                habilidades.Add(habilidad);
                habilidad = Console.ReadLine();
            }
            Console.WriteLine("Ingrese descripción del trabajo:");
            string descripcion = Console.ReadLine().Trim();
            string mensaje = username + Constantes.CaracterSeparador;
            habilidades.ForEach(m => mensaje += m + Constantes.CaracterSeparadorListas);
            mensaje = mensaje.Remove(mensaje.Length-1, 1);
            mensaje += Constantes.CaracterSeparador;
            mensaje += descripcion + Constantes.CaracterSeparador;

            Console.Clear();
            Console.WriteLine("Alta de perfil de trabajo\n");

            string respuesta = await ComunicacionServidorCliente(manejoDataSocket, mensaje, "20");
            respuesta = respuesta.Substring(0, 2);

            if (int.Parse(respuesta) == 23)
            {
                Console.WriteLine("Creacion del perfil de trabajo realizada con éxito");
            }
            else if (int.Parse(respuesta) == 22)
            {
                Console.WriteLine("Perfil de trabajo existente para este usuario");
            }
            else if (int.Parse(respuesta) == 21)
            {
                Console.WriteLine("Usuario inexistente para crear perfil de trabajo");
            }
            else
            {
                Console.WriteLine("Error desconocido"); //Esto no se debería ejecutar nunca pero lo pongo para que c# no se queje
            }

        }

        private async static Task AsociarFotoDePerfilATrabajo(ManejoStreamsHelper manejoStreams)
        {
            Console.WriteLine("Asociar foto a un perfil de trabajo");
            Console.WriteLine("Ingrese el nombre del usuario del perfil a modificar:");
            string username = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("El nombre de usuario no puede estar vacío");
                return;
            }

            string respuesta = await ComunicacionServidorCliente(manejoStreams, username, "30");
            int nroRespuesta = int.Parse(respuesta.Substring(0, 2));

            if (nroRespuesta == 32)
            {
                Console.WriteLine("El usuario no existe, ingrese nuevamente");
            }
            else
            {
                Console.WriteLine("Ingrese la ruta completa del archivo a enviar:");
                string abspath = Console.ReadLine();
                while (string.IsNullOrWhiteSpace(abspath))
                {
                    Console.WriteLine("Debe ingresar una ruta valida. Intente nuevamente:");
                    abspath = Console.ReadLine();
                }
                if(abspath.StartsWith("\"") && abspath.EndsWith("\""))
                {
                    abspath = abspath.Substring(1, abspath.Length - 2);
                }
                try
                {
                    ManejoComunArchivo fileCommonHandler = new ManejoComunArchivo(manejoStreams);
                    await fileCommonHandler.SendFile(abspath);
                } catch (Exception e)
                {
                    Console.WriteLine("Ocurrió un error: " + e.Message);
                    await manejoStreams.Recieve();
                    await manejoStreams.Recieve();
                    return;
                }
                // Este Receive es necesario porque el servidor envia un ultimo mensaje.                
                string parteFijaRespuesta = await manejoStreams.Recieve();
                string mensajeUsuarioRespuesta = await manejoStreams.Recieve();

                Console.WriteLine(mensajeUsuarioRespuesta);
            }
        }

        private async static Task ConsultarPerfilesExistentes(ManejoStreamsHelper manejoDataSocket)
        {
            Console.WriteLine("Consultar perfiles existentes");

            List<string> habilidades = new();
            List<string> palabrasDescripcion = new();
            
            bool cancel = false;
            while (!cancel)
            {
                Console.WriteLine(@"Elija una opción de busqueda:
                1 - Habilidades
                2 - Descripción
                0 - Finalizar");

                try
                {
                    int opcion = int.Parse(Console.ReadLine());
                    switch (opcion)
                    {
                        case 1:
                            habilidades = IngresarHabilidadesConsulta(habilidades);
                            break;
                        case 2:
                            palabrasDescripcion = IngresarDescripcionConsulta(palabrasDescripcion);
                            break;
                        case 0:
                            cancel = true;
                            break;
                        default:
                            Console.WriteLine("Ingrese una opción válida");
                            break;
                    }
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Solo se permiten opciones");
                }
            }
            string mensaje = string.Join(" ", habilidades) + Constantes.CaracterSeparador + string.Join(" ", palabrasDescripcion);
            await ComunicacionServidorCliente(manejoDataSocket, mensaje, "40");
        }

        private async static Task ConsultarPerfilEspecifico(ManejoStreamsHelper manejoStreams)
        {
            Console.WriteLine("Consultar perfil especifico");

            string usuarioBuscar = "";
            bool cancel = false;
            while (!cancel)
            {
                Console.WriteLine("Ingrese nombre de usuario");
                usuarioBuscar = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(usuarioBuscar))
                {
                    cancel = true;
                    break;
                }
            }
            string respuestaServidor = await ComunicacionServidorCliente(manejoStreams, usuarioBuscar, "50");
            if (respuestaServidor.Substring(3) == "\nPerfil de trabajo no existente\n") return;

            Console.WriteLine("Desea descargar la imagen de perfil (y/n)");
            string siNo = Console.ReadLine();
            if (siNo == "y")
            {
                string mensajeServidor = await ComunicacionServidorCliente(manejoStreams, usuarioBuscar, "51");
                string codigoServidor = mensajeServidor.Substring(0, 2);
                string[] respuestaServidor2 = mensajeServidor.Substring(3).Split(Constantes.CaracterSeparador);
                if (codigoServidor == "52")
                {
                    ManejoComunArchivo manejo = new ManejoComunArchivo(manejoStreams);
                    await manejo.RecibirArchivo(respuestaServidor2[1]);
                }
            } 
            else
            {
                return;
            }
        }


        private static async Task Mensajes(ManejoStreamsHelper manejoStreamsHelper, string emisor)
        {
            Console.WriteLine(
            @"Elija una opción: 
                1 - Enviar un mensaje
                2 - Leer mensajes"
            );

            int opcion = 0;
            while (opcion != 1 && opcion != 2)
            {
                try
                {
                    opcion = int.Parse(Console.ReadLine());

                    switch (opcion)
                    {
                        case 1:
                            await EnviarMensajes(manejoStreamsHelper, emisor);
                            break;
                        case 2:
                            await LeerMensajes(manejoStreamsHelper, emisor);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ingrese una opción válida");
                }
            }
        }

        private static async Task LeerMensajes(ManejoStreamsHelper manejoStreamsHelper, string emisor)
        {
            //Solicito la lista de usuarios

            string mensaje = emisor;
            string parteFija = "63";

            try
            {
                manejoStreamsHelper.Send(parteFija);
                manejoStreamsHelper.Send(mensaje);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //Recibo la lista de usuarios
            string respuesta = await manejoStreamsHelper.Recieve();
            string listaUsuarios = await manejoStreamsHelper.Recieve();

            List<string> usuarios = listaUsuarios.Split(Constantes.CaracterSeparadorListas).ToList<string>();

            usuarios.RemoveAt(usuarios.Count - 1); //El último elemento siempre es vacío por el formato con el que viene,
                                                   //entonces acá lo saco, es medio hacky pero evita que tengamos
                                                   //que hacer try catch más adelante

            Console.WriteLine("Usuarios existentes: \n");
            for (int i = 0; i < usuarios.Count; i++)
            {
                List<string> nombre = usuarios[i].Split(Constantes.CaracterSeparador).ToList();

                if (nombre[1] == "1")
                {
                    Console.WriteLine(i + " - " + nombre[0] + "[MENSAJES SIN LEER]");
                }
                else
                {
                    Console.WriteLine(i + " - " + nombre[0]);
                }
            }

            string destinatario = string.Empty;
            bool formatoOk = false;

            Console.WriteLine("Seleccione mensajes a leer: ");
            while (!formatoOk)
            {
                try
                {
                    destinatario = usuarios[int.Parse(Console.ReadLine())];
                    //Destinatario incluye el nombre de usuario, el caracter separador, y un 1 o un 0.
                    //Esto es por como quedó la string, que la usé para saber si tiene notificaciones o no
                    destinatario = destinatario.Substring(0, 2);
                    formatoOk = true;
                }
                catch (FormatException a)
                {
                    Console.WriteLine("Por favor ingrese un número");
                }
                catch (ArgumentOutOfRangeException b)
                {
                    Console.WriteLine("El número de usuario ingresado no existe");
                }
            }

            //pedirle al servidor el chat con el destinatario
            mensaje = emisor + Constantes.CaracterSeparadorListas + destinatario;
            parteFija = "61";

            try
            {
                manejoStreamsHelper.Send(parteFija);
                manejoStreamsHelper.Send(mensaje);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //Recibo el historial de mensajes
            respuesta = await manejoStreamsHelper.Recieve();
            string listaMensajes = await manejoStreamsHelper.Recieve();
            string[] mensajes = listaMensajes.Split(Constantes.CaracterSeparadorListas);

            
            //Escribo mensajes anteriores
            Console.Clear();
            Console.WriteLine("Mensajes con " + destinatario);
            Console.WriteLine("Iniciaste sesión como " + emisor);
            Console.WriteLine("-   -   -   -   -   -   -   -");
            foreach (string mensajeHistorialChat in mensajes)
            {
                Console.WriteLine(mensajeHistorialChat);
            }
        }

        private static async Task EnviarMensajes(ManejoStreamsHelper manejoStreamsHelper, string emisor)
        {
            //Solicito la lista de usuarios

            try
            {
                manejoStreamsHelper.Send("600000");
                manejoStreamsHelper.Send("");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //Recibo la lista de usuarios
            string respuesta = await manejoStreamsHelper.Recieve();
            string listaUsuarios = await manejoStreamsHelper.Recieve();

            List<string> usuarios = listaUsuarios.Split(Constantes.CaracterSeparadorListas).ToList<string>();

            usuarios.RemoveAt(usuarios.Count - 1); //El último elemento siempre es vacío por el formato con el que viene,
                                                   //entonces acá lo saco, es medio hacky pero evita que tengamos
                                                   //que hacer try catch más adelante

            Console.WriteLine("Usuarios existentes: \n");
            for (int i = 0; i < usuarios.Count; i++)
            {
                Console.WriteLine(i + " - " + usuarios[i]);
            }

            string destinatario = string.Empty;
            bool formatoOk = false;

            Console.WriteLine("Seleccione el destinatario: ");
            while (!formatoOk)
            {
                try
                {
                    destinatario = usuarios[int.Parse(Console.ReadLine())];
                    formatoOk = true;
                }
                catch (FormatException a)
                {
                    Console.WriteLine("Por favor ingrese un número");
                }
                catch (ArgumentOutOfRangeException b)
                {
                    Console.WriteLine("El número de usuario ingresado no existe");
                }
            }

            //pedirle al servidor el chat con el destinatario
            string mensaje = emisor + Constantes.CaracterSeparadorListas + destinatario;
            string parteFija = "61";

            try
            {
                manejoStreamsHelper.Send(parteFija);
                manejoStreamsHelper.Send(mensaje);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //Recibo el historial de mensajes
            respuesta = await manejoStreamsHelper.Recieve();
            string listaMensajes = await manejoStreamsHelper.Recieve();
            string[] mensajes = listaMensajes.Split(Constantes.CaracterSeparadorListas);

            //Imprimo texto a mostrar en pantalla
            Console.WriteLine("Escribiendo un mensaje a " + destinatario);
            Console.WriteLine("Iniciaste sesión como " + emisor);
            Console.WriteLine("-   -   -   -   -   -   -   -");

            //Enviar un mensaje
            string textoChat = Console.ReadLine();

            //enviar mensaje al servidor
            string mensajeChat = emisor + Constantes.CaracterSeparador + destinatario + Constantes.CaracterSeparador + textoChat;
            byte[] encodingMensajeChat = Encoding.UTF8.GetBytes(mensajeChat);
            string chatParteFija = "62" + encodingMensajeChat.Length.ToString().PadLeft(Constantes.LargoLongitudMensaje, '0');

            try
            {
                await manejoStreamsHelper.Send(chatParteFija);
                await manejoStreamsHelper.Send(mensajeChat);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }            

        private static void Desconexion(TcpClient socketCliente)
        {
            socketCliente.Close();

            Console.WriteLine("Cliente desconectado");
            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        private static List<string> IngresarHabilidadesConsulta(List<string> habilidades)
        {
            Console.WriteLine("Ingrese habilidades para filtrar (0 para terminar) ");
            string habilidad = "";
            while (habilidad != "0")
            {
                habilidad = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(habilidad))
                {
                    habilidades.Add(habilidad);
                }
            }
            habilidades.RemoveAt(habilidades.Count - 1);
            return habilidades;
        }

        private static List<string> IngresarDescripcionConsulta(List<string> palabras)
        {
            Console.WriteLine("Ingrese palabras clave para filtrar (0 para terminar) ");
            string palabraClave = "";
            while (palabraClave != "0")
            {
                palabraClave = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(palabraClave))
                {
                    palabras.Add(palabraClave);
                }
            }
            palabras.RemoveAt(palabras.Count - 1);
            return palabras;
        }

        private async static Task<string> ComunicacionServidorCliente(ManejoStreamsHelper manejoDataSocket, string mensaje, string opcion)
        {
            try
            {
                await manejoDataSocket.Send(opcion);
                await manejoDataSocket.Send(mensaje);

                string parteFijaRespuesta = await manejoDataSocket.Recieve();
                string mensajeUsuarioRespuesta = await manejoDataSocket.Recieve();

                Console.WriteLine(mensajeUsuarioRespuesta);
                return parteFijaRespuesta.Substring(0, 2) + ":" + mensajeUsuarioRespuesta;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception thrown - Message: {e.Message}");
                throw new Exception("Exception unhandeled");
            }
        }
        
    }
}

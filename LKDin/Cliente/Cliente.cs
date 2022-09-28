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
        
        static void Main(string[] args)
        {
            //Esto es porque las funciones son estáticas
            string username = "";
            //

            Console.WriteLine("Iniciando Cliente");

            string endPointClienteserverIp = settingsManager.ReadSettings(ClientConfig.endPointClienteIPconfigKey);
            int endPointClienteserverPort = int.Parse(settingsManager.ReadSettings(ClientConfig.endPointClientePortconfigKey));

            string serverIp = settingsManager.ReadSettings(ClientConfig.serverIPconfigKey);
            int serverPort = int.Parse(settingsManager.ReadSettings(ClientConfig.serverPortconfigKey));

            var socketCliente = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endpointCliente = new IPEndPoint(IPAddress.Parse(endPointClienteserverIp), endPointClienteserverPort);
            
            socketCliente.Bind(endpointCliente);

            var endpointServidor = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
           
            socketCliente.Connect(endpointServidor);
            
            ManejoSockets manejoDataSocket = new ManejoSockets(socketCliente);

            Console.WriteLine("Conexión establecida");

            bool login = false;

            while (!login)
            {
                Console.WriteLine(@"Elija una opción:
                1 - Iniciar sesión
                2 - Crear una cuenta");
                int eleccion = int.Parse(Console.ReadLine());
                (bool, string) verificacion;

                switch (eleccion)
                {
                    case 1:
                        verificacion = VerificarCredenciales(manejoDataSocket);

                        login = verificacion.Item1;
                        username = verificacion.Item2;
                        break;
                    case 2:
                        verificacion = AltaUsuario(manejoDataSocket);
                        login = verificacion.Item1;
                        username = verificacion.Item2;
                        break;
                    default:
                        Console.WriteLine("Ingrese una opción válida");
                        break;
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
                            AltaUsuario(manejoDataSocket);
                            break;
                        case 2:
                            AltaDePerfilDeTrabajo(manejoDataSocket);
                            break;
                        case 3:
                            AsociarFotoDePerfilATrabajo(manejoDataSocket, socketCliente);
                            break;
                        case 4:
                            ConsultarPerfilesExistentes(manejoDataSocket);
                            break;
                        case 5:
                            ConsultarPerfilEspecifico(manejoDataSocket, socketCliente);
                            break;
                        case 6:
                            Mensajes(manejoDataSocket, username);
                            break;
                        case 0:
                            exit = true;
                            Desconexion(socketCliente);
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

        private static (bool,string) VerificarCredenciales(ManejoSockets manejoDataSocket)
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
            byte[] mensajeServidor = Encoding.UTF8.GetBytes(mensaje);
            string e1 = mensajeServidor.Length.ToString().PadLeft(Constantes.LargoLongitudMensaje, '0');
            string e2 = "01" + e1;
            byte[] parteFija = Encoding.UTF8.GetBytes(e2);

            try
            {
                manejoDataSocket.Send(parteFija);
                manejoDataSocket.Send(mensajeServidor);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //Recibo respuesta de login
            byte[] encodingRespuesta = manejoDataSocket.Receive(Constantes.LargoParteFija);
            string respuesta = Encoding.UTF8.GetString(encodingRespuesta);

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

        private static (bool, string) AltaUsuario(ManejoSockets manejoDataSocket)
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
            string mensaje = username + "ϴ" + password;
            ComunicacionServidorCliente(manejoDataSocket, mensaje, 10);
            return (true, username);
        }

        private static void AltaDePerfilDeTrabajo(ManejoSockets manejoDataSocket)
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
            
            ComunicacionServidorCliente(manejoDataSocket, mensaje, 20);
        }

        private static void AsociarFotoDePerfilATrabajo(ManejoSockets manejoDataSocket, Socket socketCliente)
        {
            Console.WriteLine("Asociar foto a un perfil de trabajo");
            Console.WriteLine("Ingrese el nombre del usuario del perfil a modificar:");
            string username = Console.ReadLine().Trim();
            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("El nombre de usuario no puede estar vacío");
                return;
            }

            ComunicacionServidorCliente(manejoDataSocket, username, 30);

            Console.WriteLine("Ingrese la ruta completa del archivo a enviar: ");
            String abspath = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(abspath))
            {
                Console.WriteLine("Debe ingresar una ruta valida. Intente nuevamente:");
                abspath = Console.ReadLine();
            }
            ManejoComunArchivo fileCommonHandler = new ManejoComunArchivo(socketCliente);
            try
            {
                fileCommonHandler.SendFile(abspath);
            }
            catch (Exception)
            {
                Console.WriteLine("Debe ingresar una ruta valida. Intente nuevamente:");
            }
            Console.WriteLine("Se envio el archivo al Servidor");
        }

        private static void ConsultarPerfilesExistentes(ManejoSockets manejoDataSocket)
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
            string mensaje = string.Join(" ", habilidades) + "ϴ" + string.Join(" ", palabrasDescripcion);
            ComunicacionServidorCliente(manejoDataSocket, mensaje, 40);
        }

        private static void ConsultarPerfilEspecifico(ManejoSockets manejoDataSocket, Socket socket)
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
            if (ComunicacionServidorCliente(manejoDataSocket, usuarioBuscar, 50) == "\nPerfil de trabajo no existente\n") return;

            Console.WriteLine("Desea descargar la imagen de perfil (y/n)");
            string siNo = Console.ReadLine();
            if(siNo == "y")
            {
                string[] respuesta = ComunicacionServidorCliente(manejoDataSocket, usuarioBuscar, 51).Split(Constantes.CaracterSeparador);
                if (respuesta[0] == "Ok")
                {
                    ManejoComunArchivo manejo = new ManejoComunArchivo(socket);
                    manejo.RecibirArchivo(respuesta[1]);
                }
            } else
            {
                return;
            }
        }

        private static void Mensajes(ManejoSockets manejoDataSocket, string emisor)
        {
            //Solicito la
            //de usuarios
            // TODO: refactor

            byte[] encodingParteFija = Encoding.UTF8.GetBytes("600000");

            try
            {
                manejoDataSocket.Send(encodingParteFija);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //Recibo la lista de usuarios
            byte[] encodingRespuesta = manejoDataSocket.Receive(Constantes.LargoParteFija);
            string respuesta = Encoding.UTF8.GetString(encodingRespuesta);
            byte[] data = manejoDataSocket.Receive(int.Parse(respuesta.Substring(3)));
            string listaUsuarios = Encoding.UTF8.GetString(data);

            List<string> usuarios = listaUsuarios.Split(Constantes.CaracterSeparadorListas).ToList<string>();

            usuarios.RemoveAt(usuarios.Count - 1); //El último elemento siempre es vacío por el formato con el que viene,
                                                   //entonces acá lo saco, es medio hacky pero evita que tengamos
                                                   //que hacer try catch más adelante

            Console.WriteLine("Usuarios conectados: \n");
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
            byte[] mensajeServidor = Encoding.UTF8.GetBytes(mensaje);
            string parteFija = "61" + mensajeServidor.Length.ToString().PadLeft(Constantes.LargoLongitudMensaje, '0');
            encodingParteFija = Encoding.UTF8.GetBytes(parteFija);

            try
            {
                manejoDataSocket.Send(encodingParteFija);
                manejoDataSocket.Send(mensajeServidor);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //Recibo el historial de mensajes
            encodingRespuesta = manejoDataSocket.Receive(Constantes.LargoParteFija);
            respuesta = Encoding.UTF8.GetString(encodingRespuesta);
            data = manejoDataSocket.Receive(int.Parse(respuesta.Substring(3)));
            string listaMensajes = Encoding.UTF8.GetString(data);
            string[] mensajes = listaMensajes.Split(Constantes.CaracterSeparadorListas);

            //Escribo mensajes anteriores
            Console.Clear();
            Console.WriteLine("Chat con " + destinatario);
            Console.WriteLine("-   -   -   -   -   -   -   -");
            foreach (string mensajeHistorialChat in mensajes)
            {
                Console.WriteLine(mensajeHistorialChat);
            }

            //Enviar un mensaje
            string textoChat = Console.ReadLine();

            //enviar mensaje al servidor
            string mensajeChat = emisor + Constantes.CaracterSeparador + destinatario + Constantes.CaracterSeparador + textoChat;
            byte[] encodingMensajeChat = Encoding.UTF8.GetBytes(mensajeChat);
            string chatParteFija = "62" + encodingMensajeChat.Length.ToString().PadLeft(Constantes.LargoLongitudMensaje, '0');
            byte[] encodingChatParteFija = Encoding.UTF8.GetBytes(chatParteFija);

            try
            {
                manejoDataSocket.Send(encodingChatParteFija);
                manejoDataSocket.Send(encodingMensajeChat);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }            

        private static void Desconexion(Socket socketCliente)
        {
            socketCliente.Shutdown(SocketShutdown.Both);
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

        private static string ComunicacionServidorCliente(ManejoSockets manejoDataSocket, string mensaje, int opcion)
        {
            string e1, e2;
            byte[] mensajeServidor = Encoding.UTF8.GetBytes(mensaje);
            e1 = mensajeServidor.Length.ToString().PadLeft(Constantes.LargoLongitudMensaje, '0');
            e2 = opcion.ToString() + e1;
            byte[] parteFija = Encoding.UTF8.GetBytes(e2);
            try
            {
                manejoDataSocket.Send(parteFija);
                manejoDataSocket.Send(mensajeServidor);

                byte[] largoParteFijaRespuesta = manejoDataSocket.Receive(Constantes.LargoParteFija);
                string parteFijaRespuesta = Encoding.UTF8.GetString(largoParteFijaRespuesta);
                byte[] dataRespuesta = manejoDataSocket.Receive(int.Parse(parteFijaRespuesta.Substring(3)));
                string mensajeUsuarioRespuesta = Encoding.UTF8.GetString(dataRespuesta);

                Console.WriteLine(mensajeUsuarioRespuesta);
                return mensajeUsuarioRespuesta;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
    }
}

using Dominio;
using Protocolo;
using Protocolo.ManejoArchivos;
using Servidor;
using System;
using System.Collections.Immutable;
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

                int opcion = int.Parse(Console.ReadLine());

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

            // TODO: refactor
            string mensaje = username + "#" + password;
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
            // --------------------------------------------
            //Console.WriteLine("Ingrese una foto") // TODO
            // --------------------------------------------
            string mensaje = username + Constantes.CaracterSeparador;
            habilidades.ForEach(m => mensaje += m + Constantes.CaracterSeparadorListas);
            mensaje = mensaje.Remove(mensaje.Length-1, 1);
            mensaje += Constantes.CaracterSeparador;
            mensaje += descripcion + Constantes.CaracterSeparador;

            byte[] mensajeServidor = Encoding.UTF8.GetBytes(mensaje);
            string e1 = "02" + mensajeServidor.Length.ToString().PadLeft(Constantes.LargoLongitudMensaje, '0');
            byte[] parteFija = Encoding.UTF8.GetBytes(e1);

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

            byte[] mensajeServidor = Encoding.UTF8.GetBytes(username);
            string e1 = "03" + mensajeServidor.Length.ToString().PadLeft(Constantes.LargoLongitudMensaje, '0');
            byte[] parteFija = Encoding.UTF8.GetBytes(e1);

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

            Console.WriteLine("Ingrese la ruta completa del archivo a enviar: ");
            String abspath = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(abspath))
            {
                Console.WriteLine("Debe ingresar una ruta valida. Intente nuevamente:");
                abspath = Console.ReadLine();
            }
            ManejoComunArchivo fileCommonHandler = new ManejoComunArchivo(socketCliente);
            fileCommonHandler.SendFile(abspath);
            Console.WriteLine("Se envio el archivo al Servidor");

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
    }
}

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
            Console.WriteLine("Escriba un meensaje para el Servidor");
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
                    case 4:
                        ConsultarPerfilesExistentes(manejoDataSocket);
                        break;
                    case 5:
                        ConsultarPerfilEspecifico(manejoDataSocket);
                        break;
                    case 6:
                        Mensajes(manejoDataSocket);
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

        private static void AltaUsuario(ManejoSockets manejoDataSocket)
        {
            Console.WriteLine("Alta de usuario");

            Console.WriteLine("Escriba el nombre de usuario");
            string username = Console.ReadLine().Trim();
            Console.WriteLine("Escriba la contraseña");
            string password = Console.ReadLine().Trim();
            
            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("El nombre de usuario no puede estar vacío");
                return;
            }
            else if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("La contraseña no puede estar vacía");
                return;
            }
            string mensaje = username + "ϴ" + password;
            ComunicacionServidorCliente(manejoDataSocket, mensaje, 10);
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

            byte[] mensajeServidor = Encoding.UTF8.GetBytes(username);
            string e1 = "30" + mensajeServidor.Length.ToString().PadLeft(Constantes.LargoLongitudMensaje, '0');
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

        private static void ConsultarPerfilEspecifico(ManejoSockets manejoDataSocket)
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
            ComunicacionServidorCliente(manejoDataSocket, usuarioBuscar, 50);
        }

        private static void Mensajes(ManejoSockets manejoDataSocket)
        {
            throw new NotImplementedException();
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

        private static void ComunicacionServidorCliente(ManejoSockets manejoDataSocket, string mensaje, int opcion)
        {
            byte[] parteFija = { };
            byte[] mensajeServidor = { };
            string e1, e2;
            
            switch (opcion)
            {
                case 10:
                    mensajeServidor = Encoding.UTF8.GetBytes(mensaje);
                    e1 = mensajeServidor.Length.ToString().PadLeft(Constantes.LargoLongitudMensaje, '0');
                    e2 = opcion.ToString() + e1;
                    parteFija = Encoding.UTF8.GetBytes(e2);
                    break;
                case 20:
                    mensajeServidor = Encoding.UTF8.GetBytes(mensaje);
                    e1 = mensajeServidor.Length.ToString().PadLeft(Constantes.LargoLongitudMensaje, '0');
                    e2 = opcion.ToString() + e1;
                    parteFija = Encoding.UTF8.GetBytes(e2);
                    break;
                case 40:
                    mensajeServidor = Encoding.UTF8.GetBytes(mensaje);
                    e1 = mensajeServidor.Length.ToString().PadLeft(Constantes.LargoLongitudMensaje, '0');
                    e2 = opcion.ToString() + e1;
                    parteFija = Encoding.UTF8.GetBytes(e2);
                    break;
                case 50:
                    mensajeServidor = Encoding.UTF8.GetBytes(mensaje);
                    e1 = mensajeServidor.Length.ToString().PadLeft(Constantes.LargoLongitudMensaje, '0');
                    e2 = opcion.ToString() + e1;
                    parteFija = Encoding.UTF8.GetBytes(e2);
                    break;
            }

            try
            {
                manejoDataSocket.Send(parteFija);
                manejoDataSocket.Send(mensajeServidor);

                byte[] largoParteFijaRespuesta = manejoDataSocket.Receive(Constantes.LargoParteFija);
                string parteFijaRespuesta = Encoding.UTF8.GetString(largoParteFijaRespuesta);
                byte[] dataRespuesta = manejoDataSocket.Receive(int.Parse(parteFijaRespuesta.Substring(3)));
                string mensajeUsuarioRespuesta = Encoding.UTF8.GetString(dataRespuesta);

                Console.WriteLine(mensajeUsuarioRespuesta);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
    }
}

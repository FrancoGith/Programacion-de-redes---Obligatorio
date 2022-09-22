using Protocolo;
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
                //6 opciones

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
                        AsociarFotoPerfilTrabajo(manejoDataSocket);
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
        }

        private static void AltaDePerfilDeTrabajo(ManejoSockets manejoDataSocket)
        {
            throw new NotImplementedException();
        }

        private static void AsociarFotoPerfilTrabajo(ManejoSockets manejoDataSocket)
        {
            throw new NotImplementedException();
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
                0 - Cancelar");

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

            // TODO: refactor

            string mensaje = string.Join(" ", habilidades) + "ϴ" + string.Join(" ", palabrasDescripcion);
            byte[] mensajeServidor = Encoding.UTF8.GetBytes(mensaje);
            string e1 = mensajeServidor.Length.ToString().PadLeft(Constantes.LargoLongitudMensaje, '0');
            string e2 = "04" + e1;
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
        }

        private static void ConsultarPerfilEspecifico(ManejoSockets manejoDataSocket)
        {
            throw new NotImplementedException();
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
    }
}

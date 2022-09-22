using Protocolo;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Protocolo;

namespace Cliente
{
    class Cliente
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Iniciando Cliente");
            var socketCliente = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endpointCliente = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            socketCliente.Bind(endpointCliente);

            var endpointServidor = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 14000);

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

        private static void Mensajes(ManejoSockets manejoDataSocket)
        {
            //Solicito la lista de usuarios
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
            List<string> usuarios = listaUsuarios.Split('#').ToList<string>();
            
            usuarios.RemoveAt(usuarios.Count - 1); //El último elemento siempre es vacío por el formato con el que viene,
                                                   //entonces acá lo saco, es medio hacky pero evita que tengamos
                                                   //que hacer try catch más adelante

            Console.WriteLine("Usuarios conectados: \n");
            for (int i = 0; i < usuarios.Count; i++)
            {
                Console.WriteLine(i + " - " + usuarios[i]);
            }

            string destinatario = string.Empty;
            string emisor = string.Empty;
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

            Console.WriteLine("\nSeleccione el emisor: ");
            formatoOk = false;
            while (!formatoOk)
            {
                try
                {
                    emisor = usuarios[int.Parse(Console.ReadLine())];
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
            string mensaje = emisor + "#" + destinatario;
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
            string[] mensajes = listaMensajes.Split('#');

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
            string mensajeChat = emisor + "#" + destinatario + "#" + textoChat;
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

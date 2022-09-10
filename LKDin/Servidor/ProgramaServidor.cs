﻿using System;
using System.Data;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Servidor
{
    class ProgramaServidor
    {
        static int cantidadClientes = 2;
        private static DatosServidor datosServidor = new() { ListaUsuarios = new() };
        
        static void Main(string[] args)
        {
            Console.WriteLine("Levantando Servidor");
            var socketServidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 14000);
            socketServidor.Bind(endpoint);

            socketServidor.Listen(100);

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
            while (clienteConectado)
            {
                try
                {
                    byte[] largodata = new byte[4];
                    socketCliente.Receive(largodata);
                    int largo = BitConverter.ToInt32(largodata);

                    byte[] data = new byte[largo];
                    int cantDatos = socketCliente.Receive(data);
                    
                    if (cantDatos == 0)
                    {
                        clienteConectado = false;
                    }
                    else
                    {
                        string mensajeCliente = Encoding.UTF8.GetString(data);
                        Console.WriteLine($"Cliente dice: {mensajeCliente}");

                        if (mensajeCliente == "1")
                        {
                            Console.WriteLine($"Opcion elegida por el cliente : {mensajeCliente}");
                            AltaDeUsuario(socketCliente);
                        }

                    }
                }
                catch (SocketException e)
                {
                    clienteConectado = false;
                }
            }
            Console.WriteLine("Cliente desconectado");
        }

        static void AltaDeUsuario(Socket socketCliente)
        {
            byte[] largodata1 = new byte[4];
            socketCliente.Receive(largodata1);
            int largo1 = BitConverter.ToInt32(largodata1);

            byte[] data1 = new byte[largo1];
            int cantDatos1 = socketCliente.Receive(data1);

            Console.WriteLine($"Cliente dice: {Encoding.UTF8.GetString(data1)}");
            datosServidor.ListaUsuarios.Add(Encoding.UTF8.GetString(data1));
        }
        
    }
}

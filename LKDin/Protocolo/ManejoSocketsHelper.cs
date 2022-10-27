﻿using System.Drawing;
using System.Net.Sockets;
using System.Text;

namespace Protocolo
{
    public class ManejoStreamsHelper
    {
        private readonly NetworkStream _stream;

        public ManejoStreamsHelper(NetworkStream stream)
        {
            _stream = stream;
        }

        public async Task Send(string mensaje)
        {
            byte[] data = Encoding.UTF8.GetBytes(mensaje);
            byte[] dataLength = BitConverter.GetBytes(data.Length);

            await _stream.WriteAsync(dataLength, 0, Constantes.LargoLongitudMensaje);

            await _stream.WriteAsync(data, 0, data.Length);
        }

        public async Task<string> Recieve()
        {
            var dataLength = new byte[Constantes.LargoLongitudMensaje];
            int totalReceived = 0;
            while (totalReceived < Constantes.LargoLongitudMensaje)
            {
                var received = await _stream.ReadAsync(dataLength, totalReceived, Constantes.LargoLongitudMensaje - totalReceived);
                if (received == 0)
                {
                    throw new SocketException();
                }
                totalReceived += received;
            }
            int length = BitConverter.ToInt32(dataLength);
            byte[] data = new byte[length];
            totalReceived = 0;
            while (totalReceived < length)
            {
                int received = await _stream.ReadAsync(data, totalReceived, length - totalReceived);
                if (received == 0)
                {
                    throw new SocketException();
                }
                totalReceived += received;
            }
            return Encoding.UTF8.GetString(data);
        }
    }

}

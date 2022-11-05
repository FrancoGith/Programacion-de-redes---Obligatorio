using System.Drawing;
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

        public async Task Send(byte[] buffer)
        {
            byte[] dataLength = BitConverter.GetBytes(buffer.Length);
            await _stream.WriteAsync(dataLength, 0, Constantes.LargoLongitudMensaje);
            await _stream.WriteAsync(buffer, 0, buffer.Length);
        }

        public async Task<byte[]> Receive(int size)
        {
            var dataLength = new byte[size];
            int totalReceived = 0;
            while (totalReceived < size)
            {
                var received = await _stream.ReadAsync(dataLength, totalReceived, size - totalReceived);
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
            return data;
        }
    }

}
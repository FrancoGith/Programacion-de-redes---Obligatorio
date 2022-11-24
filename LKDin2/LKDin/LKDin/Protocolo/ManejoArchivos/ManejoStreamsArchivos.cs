using System.Net.Sockets;

namespace Protocolo.ManejoArchivos
{
    public class ManejoStreamsArchivos
    {
        private readonly Socket _socket;

        public ManejoStreamsArchivos(Socket socket)
        {
            _socket = socket;
        }

        public void Send(byte[] buffer)
        {
            int offset = 0;
            int size = buffer.Length;

            while (offset < size)
            {
                int sent = _socket.Send(buffer, offset, size - offset, SocketFlags.None);

                if (sent == 0)
                {
                    throw new SocketException();
                }
                offset += sent;
            }
        }

        public byte[] Receive(int size)
        {
            int offset = 0;
            byte[] buffer = new byte[size];

            while (offset < size)
            {
                int recibido = _socket.Receive(buffer, offset, size - offset, SocketFlags.None);

                if (recibido == 0)
                {
                    throw new SocketException();
                }
                offset += recibido;
            }

            return buffer;
        }
    }

}

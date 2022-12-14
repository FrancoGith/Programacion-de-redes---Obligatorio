using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Protocolo.ManejoArchivos
{
    public class ManejoComunArchivo
    {
        private readonly ManejoStreamsArchivos _socketHelper;

        public ManejoComunArchivo(Socket socket)
        {
            _socketHelper = new ManejoStreamsArchivos(socket);
        }

        public async Task SendFile(string path)
        {
            if (VerificacionExistenciaArchivos.FileExists(path))
            {
                string fileName = VerificacionExistenciaArchivos.GetFileName(path);
                //---------------- Solo envio el nombre porque necesito la extension
                // ---> Enviar el largo del nombre del archivo
                _socketHelper.Send(ManejoConversiones.ConvertIntToBytes(fileName.Length));
                // ---> Enviar el nombre del archivo
                _socketHelper.Send(ManejoConversiones.ConvertStringToBytes(fileName));

                // ---> Obtener el tamaño del archivo

                // falla en el long fileSize linea 30 manejocomunarchivo

                long fileSize = VerificacionExistenciaArchivos.GetFileSize(path);
                // ---> Enviar el tamaño del archivo
                byte[] convertedFileSize = ManejoConversiones.ConvertLongToBytes(fileSize);
                _socketHelper.Send(convertedFileSize);
                // ---> Enviar el archivo (pero con file stream)
                await SendFileWithStream(fileSize, path);
            }
            else
            {
                throw new Exception("File does not exist");
            }
        }

        public async Task<string> RecieveFile(string _nombreArchivo)
        {
            // --------------------------------------------------No me importa el nombre porque lo sobrescribo
            // ---> Recibir el largo del nombre del archivo
            int fileNameSize = ManejoConversiones.ConvertBytesToInt(
            _socketHelper.Receive(ManejoTamanoArchivos.FixedDataSize));
            // ---> Recibir el nombre del archivo
            string fileName = ManejoConversiones.ConvertBytesToString(_socketHelper.Receive(fileNameSize));
            // ---> Recibir el largo del archivo
            string extension = Path.GetExtension(fileName);
            long fileSize = ManejoConversiones.ConvertBytesToLong(
            _socketHelper.Receive(ManejoTamanoArchivos.FixedFileSize));
            // ---> Recibir el archivo
            string archivoAGuardar = _nombreArchivo + extension;
            await ReceiveFileWithStreams(fileSize, archivoAGuardar);
            return archivoAGuardar;
        }

        private async Task SendFileWithStream(long fileSize, string path)
        {
            long fileParts = ManejoTamanoArchivos.CalcularCantidadDePartes(fileSize);
            long offset = 0;
            long currentPart = 1;

            //Mientras tengo un segmento a enviar
            while (fileSize > offset)
            {
                byte[] data;
                //Es el último segmento?
                if (currentPart == fileParts)
                {
                    int lastPartSize = (int)(fileSize - offset);
                    //1- Leo de disco el último segmento
                    //2- Guardo el último segmento en un buffer
                    data = await ManejoFileStream.Read(path, offset, lastPartSize); //Puntos 1 y 2
                    offset += lastPartSize;
                }
                else
                {
                    //1- Leo de disco el segmento
                    //2- Guardo ese segmento en un buffer
                    data = await ManejoFileStream.Read(path, offset, ManejoTamanoArchivos.MaxPacketSize);
                    offset += ManejoTamanoArchivos.MaxPacketSize;
                }

                _socketHelper.Send(data); //3- Envío ese segmento a travez de la red
                currentPart++;
            }
        }

        private async Task ReceiveFileWithStreams(long fileSize, string fileName)
        {
            long fileParts = ManejoTamanoArchivos.CalcularCantidadDePartes(fileSize);
            long offset = 0;
            long currentPart = 1;

            //Mientras tengo partes para recibir
            while (fileSize > offset)
            {
                byte[] data;
                //1- Me fijo si es la ultima parte
                if (currentPart == fileParts)
                {
                    //1.1 - Si es, recibo la ultima parte
                    int lastPartSize = (int)(fileSize - offset);
                    data = _socketHelper.Receive(lastPartSize);
                    offset += lastPartSize;
                }
                else
                {
                    //2.2- Si no, recibo una parte cualquiera
                    data = _socketHelper.Receive(ManejoTamanoArchivos.MaxPacketSize);
                    offset += ManejoTamanoArchivos.MaxPacketSize;
                }
                //3- Escribo esa parte del archivo a disco
                await ManejoFileStream.Write(fileName, data);
                currentPart++;
            }
        }
    }
}

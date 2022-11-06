using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocolo.ManejoArchivos
{
    internal static class ManejoFileStream
    {
        public static async Task<byte[]> Read(string path, long offset, int length)
        {
            if (VerificacionExistenciaArchivos.FileExists(path))
            {
                var data = new byte[length];

                using var fs = new FileStream(path, FileMode.Open) { Position = offset };
                var bytesRead = 0;
                while (bytesRead < length)
                {
                    var read = await fs.ReadAsync(data, bytesRead, length - bytesRead);
                    if (read == 0)
                        throw new Exception("Error reading file");
                    bytesRead += read;
                }

                return data;
            }

            throw new Exception("File does not exist");
        }

        public static async Task Write(string fileName, byte[] data)
        {
            var fileMode = VerificacionExistenciaArchivos.FileExists(fileName) ? FileMode.Append : FileMode.Create;
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            using var fs = new FileStream(fileName, fileMode);
            await fs.WriteAsync(data, 0, data.Length);
        }
    }
}

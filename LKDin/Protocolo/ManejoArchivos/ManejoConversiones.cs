using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocolo.ManejoArchivos
{
    internal static class ManejoConversiones
    {
        public static byte[] ConvertStringToBytes(string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        public static string ConvertBytesToString(byte[] value)
        {
            return Encoding.UTF8.GetString(value);
        }

        public static byte[] ConvertIntToBytes(int value)
        {
            return BitConverter.GetBytes(value);
        }

        public static int ConvertBytesToInt(byte[] value)
        {
            return BitConverter.ToInt32(value);
        }

        public static byte[] ConvertLongToBytes(long value)
        {
            return BitConverter.GetBytes(value);
        }

        public static long ConvertBytesToLong(byte[] value)
        {
            return BitConverter.ToInt64(value);
        }
    }
}

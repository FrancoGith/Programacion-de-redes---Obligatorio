using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Mensajes
{
    public class HistorialChat
    {
        public (string, string) usuarios;
        public List<string> mensajes;

        public HistorialChat()
        { 
            mensajes = new List<string>();
        }
    }
}

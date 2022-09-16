using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocolo
{
    internal class Constantes
    {
        public const int LargoCodigo = 2;
        public const int LargoLongitudMensaje = 4;


        public const int LargoHeader = LargoCodigo + LargoLongitudMensaje;
    }
}

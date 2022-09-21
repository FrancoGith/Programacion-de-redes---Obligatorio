using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class PerfilTrabajo
    {
        public Usuario Usuario { get; set; }
        public List<string> Habilidades { get; set; }
        public string Descripcion { get; set; }
        public string Foto { get; set; }
    }
}

using Google.Protobuf.Collections;
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

        public PerfilTrabajo()
        {
            Usuario = new Usuario();
            Habilidades = new List<string>();
            Descripcion = String.Empty;
            Foto = String.Empty;
        }

        public PerfilTrabajo(List<string> habilidades, string descripcion)
        {
            Habilidades=habilidades;
            Descripcion=descripcion;
        }

        public string GetSearchData()
        {
            return string.Join(" ", Habilidades) + " " + Descripcion;
        }
    }
}

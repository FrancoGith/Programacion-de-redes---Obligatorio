using Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servidor
{
    public class DatosServidor
    {
        public List<Usuario>? Usuarios { get; set; }
        public List<PerfilTrabajo>? PerfilesTrabajo { get; set; }
        public DatosServidor()
        {
            Usuarios = new List<Usuario>();
            PerfilesTrabajo = new List<PerfilTrabajo>();
        }

        public Usuario GetUsuario(string _username)
        {
            Usuario retorno = Usuarios.Find(a => a.Username == _username);
            if (retorno == null) throw new Exception("Usuario no encontrado");
            else return retorno;
        }

        public PerfilTrabajo GetPerfilTrabajo(string _username)
        {
            PerfilTrabajo retorno = PerfilesTrabajo.Find(a => a.Usuario.Username == _username);
            if (retorno == null) throw new Exception("Perfil no encontrado");
            else return retorno;
        }
    }
}

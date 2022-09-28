using Dominio;
using Dominio.Mensajes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servidor
{
    public class DatosServidor
    {
        private List<Usuario>? Usuarios { get; set; }
        private List<PerfilTrabajo>? PerfilesTrabajo { get; set; }
        private List<HistorialChat>? ListaHistoriales { get; set; }
        public DatosServidor()
        {
            Usuarios = new List<Usuario>();
            PerfilesTrabajo = new List<PerfilTrabajo>();
            ListaHistoriales = new List<HistorialChat>();
        }

        public void AgregarUsuario(string username, string password)
        {
            lock (Usuarios)
            {
                Usuarios.Add(new Usuario() { Username = username, Password = password });
            }
        }

        public void AgregarPerfilTrabajo(Usuario usuario, List<string> habilidades, string descripcion)
        {
            lock (PerfilesTrabajo)
            {
                PerfilesTrabajo.Add(new PerfilTrabajo() { Usuario = usuario, Habilidades = habilidades, Descripcion = descripcion });
            }
        }

        public void AgregarHistorial(HistorialChat historialDevolver)
        {
            lock (ListaHistoriales)
            {
                ListaHistoriales.Add(historialDevolver);
            }
        }

        public Usuario GetUsuario(string _username)
        {
            return Usuarios.Find(a => a.Username == _username);
        }

        public PerfilTrabajo GetPerfilTrabajo(string _username)
        {
            return PerfilesTrabajo.Find(a => a.Usuario.Username == _username);
        }

        public List<PerfilTrabajo> GetPerfilesTrabajo()
        {
            return PerfilesTrabajo;
        }

        public List<Usuario> GetUsuarios()
        {
            return Usuarios;
        }

        public HistorialChat GetHistorial(string[] usuarios)
        {
            return ListaHistoriales.FirstOrDefault(x => x.usuarios.Equals((usuarios[0], usuarios[1])) || x.usuarios.Equals((usuarios[1], usuarios[0])));
        }
    }
}

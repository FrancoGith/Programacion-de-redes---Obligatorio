using Dominio;
using Dominio.Mensajes;
using Google.Protobuf.Collections;
using GrpcServerProgram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrpcServerProgram.Servidor
{
    public class DatosServidor
    {

        private static DatosServidor instanciaDatos;
        private static readonly object singletonLock = new object();

        public static DatosServidor GetInstancia()
        {
            lock (singletonLock)
            {
                if (instanciaDatos == null)
                {
                    instanciaDatos = new DatosServidor();
                }
            }
            return instanciaDatos;
        }

        private List<Usuario>? Usuarios { get; set; }
        private List<PerfilTrabajo>? PerfilesTrabajo { get; set; }
        private List<HistorialChat>? ListaHistoriales { get; set; }
        public DatosServidor()
        {
            Usuarios = new List<Usuario>();
            PerfilesTrabajo = new List<PerfilTrabajo>();
            ListaHistoriales = new List<HistorialChat>();
        }

        public List<Usuario> ObtenerUsuarios()
        {
            return Usuarios;
        }

        public void AgregarUsuario(string username, string password)
        {
            lock (Usuarios)
            {
                if (Usuarios.FirstOrDefault(u => u.Username == username) != null)
                {
                    throw new ArgumentException("Usuario existente, intente con otro nombre.");
                }
                Usuarios.Add(new Usuario() { Username = username, Password = password });
            }
        }

        public void EliminarUsuario(string username)
        {
            lock (Usuarios)
            {
                Usuario usuarioEncontrado = Usuarios.FirstOrDefault(u => u.Username == username);
                if (usuarioEncontrado == null)
                {
                    throw new ArgumentException("Usuario inexistente.");
                }
                else if (usuarioEncontrado.Conectado)
                {
                    throw new ArgumentException("No se puede eliminar el usuario porque este se encuentra conectado.");
                }
                else
                {
                    Usuarios.RemoveAll(u => u.Username == username);
                }
            }
        }

        public void EliminarPerfil(string username)
        {
            lock (PerfilesTrabajo)
            {
                Usuario usuarioEncontrado = Usuarios.FirstOrDefault(u => u.Username == username);
                if (usuarioEncontrado == null)
                {
                    throw new ArgumentException("Usuario inexistente.");
                }
                else
                {
                    PerfilesTrabajo.RemoveAll(p => p.Usuario.Username == username);
                }
            }
        }

        public void AgregarPerfilTrabajo(string username, List<string> habilidades, string descripcion)
        {
            Usuario usuario = Usuarios.FirstOrDefault(u => u.Username == username);

            if (usuario != null)
            {
                if (GetPerfilTrabajo(username) != null)
                {
                    throw new ArgumentException("Perfil de trabajo existente para este usuario.");
                }
                lock (PerfilesTrabajo)
                {
                    PerfilesTrabajo.Add(new PerfilTrabajo() { Usuario = usuario, Habilidades = habilidades, Descripcion = descripcion });
                }
            }
            else
            {
                throw new ArgumentException("Usuario inexistente.");
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

        public List<HistorialChat> GetHistoriales(string usuario)
        {
            return ListaHistoriales.Where(x => x.usuarios.Item1 == usuario || x.usuarios.Item2 == usuario).ToList();

        }

        internal void ModificarUsuario(string userId, Usuario user)
        {
            lock (Usuarios)
            {
                string nombreViejo = "";
                Usuario usuarioEncontrado = GetUsuario(userId);
                if (usuarioEncontrado == null)
                {
                    throw new ArgumentException("Usuario inexistente.");
                }
                else if (usuarioEncontrado.Conectado)
                {
                    throw new ArgumentException("No se puede modificar el usuario porque este se encuentra conectado.");
                }
                else
                {
                    nombreViejo = usuarioEncontrado.Username;
                    usuarioEncontrado.Username = user.Username;
                    usuarioEncontrado.Password = user.Password;
                }
            }

            lock (ListaHistoriales)
            {
                foreach (HistorialChat historialChat in ListaHistoriales)
                {
                    if (historialChat.usuarios.Item1 == userId)
                    {
                        historialChat.usuarios.Item1 = user.Username;
                    }
                    if (historialChat.usuarios.Item2 == userId)
                    {
                        historialChat.usuarios.Item1 = user.Username;
                    }
                    if (historialChat.ultimoEnHablar == userId)
                    {
                        historialChat.ultimoEnHablar = user.Username;
                    }

                    List<string> nuevosMensajes = new();

                    foreach (string mensaje in historialChat.mensajes)
                    {
                        var regex = new Regex(Regex.Escape(userId));
                        nuevosMensajes.Add(regex.Replace(mensaje, user.Username, 1));
                    }
                    historialChat.mensajes = nuevosMensajes;
                }
            }
        }

        internal void ModificarPerfil(string userId, PerfilTrabajo perfilTrabajo)
        {
            lock (PerfilesTrabajo)
            {
                PerfilTrabajo perfilEncontrado = GetPerfilTrabajo(userId);
                if (perfilEncontrado == null)
                {
                    throw new ArgumentException("Perfil de trabajo inexistente.");
                }
                else
                {
                    perfilEncontrado.Habilidades = perfilTrabajo.Habilidades;
                    perfilEncontrado.Descripcion = perfilTrabajo.Descripcion;
                }
            }
        }

        public void EliminarFoto(string username)
        {
            lock (PerfilesTrabajo)
            {
                PerfilTrabajo perfilEncontrado = GetPerfilTrabajo(username);
                if (perfilEncontrado == null)
                {
                    throw new ArgumentException("Perfil de trabajo inexistente.");
                }
                else if (perfilEncontrado.Foto == "")
                {
                    throw new ArgumentException("El perfil no tiene imagen.");
                }
                else
                {
                    perfilEncontrado.Foto = "";
                }
            }
        }
    }
}

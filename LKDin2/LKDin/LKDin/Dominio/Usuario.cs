namespace Dominio
{
    public class Usuario
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public bool Conectado { get; set; }

        public Usuario() { }
    }
}
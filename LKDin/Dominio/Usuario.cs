namespace Dominio
{
    public class Usuario
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public List<string> Mensajes { get; set; }
    }
}
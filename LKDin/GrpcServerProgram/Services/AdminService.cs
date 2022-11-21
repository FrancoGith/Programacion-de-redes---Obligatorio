using Grpc.Core;
using GrpcServerProgram;
using Servidor;
using System.Text.Json;

namespace GrpcServerProgram.Services
{
    public class AdminService : Admin.AdminBase
    {
        private readonly ILogger<AdminService> _logger;
        DatosServidor datosServidor;

        public AdminService(ILogger<AdminService> logger)
        {
            _logger = logger;
        }

        public override Task<MessageReply> GetUsers(GetMessage message, ServerCallContext context)
        {
            datosServidor = DatosServidor.GetInstancia();
            string response = JsonSerializer.Serialize(datosServidor.ObtenerUsuarios());
            return Task.FromResult(new MessageReply
            {
                Message = response,
            });
        }

        public override Task<MessageReply> PostUser(UserDTO user, ServerCallContext context)
        {
            datosServidor = DatosServidor.GetInstancia();
            datosServidor.AgregarUsuario(user.Username, user.Password);
            return Task.FromResult(new MessageReply
            {
                Message = "Usuario creado: " + user.Username + " " + user.Password,
            });
        }

        public override Task<MessageReply> DeleteUser(Id username, ServerCallContext context)
        {
            datosServidor = DatosServidor.GetInstancia();
            datosServidor.EliminarUsuario(username.Id_);
            return Task.FromResult(new MessageReply
            {
                Message = "Usuario " + username.Id_ + " eliminado",
            });
        }
    }
}
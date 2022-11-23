using Dominio;
using Google.Protobuf.Collections;
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
            var response = JsonSerializer.Serialize(datosServidor.ObtenerUsuarios());
            Console.WriteLine(response);
            return Task.FromResult(new MessageReply
            {
                Message = response,
            });
        }

        public override Task<MessageReply> PostUser(UserDTO user, ServerCallContext context)
        {
            try
            {
                datosServidor = DatosServidor.GetInstancia();
                datosServidor.AgregarUsuario(user.Username, user.Password);
                return Task.FromResult(new MessageReply
                {
                    Message = "Usuario creado: " + user.Username + " " + user.Password,
                });
            }
            catch (ArgumentException error)
            {
                return Task.FromResult(new MessageReply
                {
                    Message = error.Message,
                });
            }
        }

        public override Task<MessageReply> ModifyUser(ModifyUserDTO modifyUserDTO, ServerCallContext context)
        {
            try
            {
                Usuario usuario = new()
                {
                    Username = modifyUserDTO.User.Username,
                    Password = modifyUserDTO.User.Password
                };

                datosServidor = DatosServidor.GetInstancia();
                datosServidor.ModificarUsuario(modifyUserDTO.UserId, usuario);
                return Task.FromResult(new MessageReply
                {
                    Message = "Usuario " + modifyUserDTO.UserId+ " modificado con " + modifyUserDTO.User,
                });
            }
            catch (ArgumentException error)
            {
                return Task.FromResult(new MessageReply
                {
                    Message = error.Message,
                });
            }
        }

        public override Task<MessageReply> DeleteUser(Id username, ServerCallContext context)
        {
            try
            {
                datosServidor = DatosServidor.GetInstancia();
                datosServidor.EliminarUsuario(username.Username);
                return Task.FromResult(new MessageReply
                {
                    Message = "Usuario eliminado: " + username.Username,
                });
            }
            catch (ArgumentException error)
            {
                return Task.FromResult(new MessageReply
                {
                    Message = error.Message,
                });
            }
        }

        public override Task<MessageReply> GetProfiles(GetMessage message, ServerCallContext context)
        {
            try
            {
                datosServidor = DatosServidor.GetInstancia();
                var response = JsonSerializer.Serialize(datosServidor.GetPerfilesTrabajo());
                return Task.FromResult(new MessageReply
                {
                    Message = response,
                });
            }
            catch (Exception e)
            {
                return Task.FromResult(new MessageReply
                {
                    Message = e.Message,
                });
            }
        }

        public override Task<MessageReply> PostProfile(ProfileDTO profileDTO, ServerCallContext context)
        {
            try
            {
                List<string> habilidades = new();
                foreach (var habilidad in profileDTO.Habilidades)
                {
                    habilidades.Add(habilidad);
                }

                datosServidor = DatosServidor.GetInstancia();
                datosServidor.AgregarPerfilTrabajo(profileDTO.ProfileUserId, habilidades, profileDTO.Descripcion);
                return Task.FromResult(new MessageReply
                {
                    Message = "Perfil creado para el usuario: " + profileDTO.ProfileUserId,
                });
            }
            catch (ArgumentException)
            {
                return Task.FromResult(new MessageReply
                {
                    Message = "Usuario inexistente",
                });
            }
        }

        public override Task<MessageReply> ModifyProfile(ModifyProfileDTO modifyProfileDTO, ServerCallContext context)
        {
            try
            {
                List<string> habilidades = new();
                foreach (string habilidad in modifyProfileDTO.Habilidades)
                {
                    habilidades.Add(habilidad);
                }

                PerfilTrabajo perfilTrabajo = new PerfilTrabajo(
                    habilidades,
                    modifyProfileDTO.Descripcion);

                datosServidor = DatosServidor.GetInstancia();
                datosServidor.ModificarPerfil(modifyProfileDTO.ProfileUserId, perfilTrabajo);

                return Task.FromResult(new MessageReply
                {
                    Message = "Perfil modificado para el usuario" + modifyProfileDTO.ProfileUserId,
                });
            }
            catch (ArgumentException)
            {
                return Task.FromResult(new MessageReply
                {
                    Message = "Usuario inexistente",
                });
            }
        }

        public override Task<MessageReply> DeleteProfile(Id username, ServerCallContext context)
        {
            try
            {
                datosServidor = DatosServidor.GetInstancia();
                datosServidor.EliminarPerfil(username.Username);
                return Task.FromResult(new MessageReply
                {
                    Message = "Perfil del usuario: " + username.Username + " eliminado.",
                });
            }
            catch (ArgumentException)
            {
                return Task.FromResult(new MessageReply
                {
                    Message = "Perfil del usuario inexistente",
                });
            }
        }

        public override Task<MessageReply> DeleteImage(Id username, ServerCallContext context)
        {
            try
            {
                // TODO: ELIMINAR FOTO DE LA CARPETA DEL SERVER
                datosServidor = DatosServidor.GetInstancia();
                datosServidor.EliminarFoto(username.Username);
                return Task.FromResult(new MessageReply
                {
                    Message = "Imagen del perfil del usuario: " + username.Username + " eliminado.",
                });
            }
            catch (ArgumentException error)
            {
                return Task.FromResult(new MessageReply
                {
                    Message = error.Message,
                });
            }
        }
    }
}
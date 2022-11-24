using AdminServer.NewFolder;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using Protocolo;

namespace AdminServer.Controllers
{
    [Route("api/")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private Admin.AdminClient adminClient;
        private string grpcURL;
        static readonly SettingsManager settingsManager = new SettingsManager();

        public AdminController()
        {
            AppContext.SetSwitch(
                  "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            grpcURL = settingsManager.ReadSettings(ServerConfig.GrpcURL);
        }

        [HttpGet("usuarios")]
        public async Task<ActionResult> ObtenerUsuarios()
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(grpcURL);
                adminClient = new Admin.AdminClient(channel);
                var reply = await adminClient.GetUsersAsync(new GetMessage());
                return Ok(reply);
            }
            catch (ArgumentException error)
            {
                return BadRequest(error.Message);
            }
        }

        [HttpPost("usuarios")]
        public async Task<ActionResult> CrearUsuario([FromBody] UserDTO user)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            adminClient = new Admin.AdminClient(channel);
            var reply = await adminClient.PostUserAsync(user);
            return Ok(reply.Message);
        }

        [HttpPut("usuarios")]
        public async Task<ActionResult> ModificarUsuario([FromBody] ModifyUserDTO user)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            adminClient = new Admin.AdminClient(channel);
            var reply = await adminClient.ModifyUserAsync(user);
            return Ok(reply.Message);
        }

        [HttpDelete("usuarios/{id}")]
        public async Task<ActionResult> EliminarUsuario([FromRoute] string id)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(grpcURL);
                adminClient = new Admin.AdminClient(channel);
                var reply = await adminClient.DeleteUserAsync(new Id { Username = id });
                return Ok(reply.Message);
            }
            catch (ArgumentException error)
            {
                return BadRequest(error.Message);
            }
        }

        [HttpGet("perfiles")]
        public async Task<ActionResult> ObtenerPerfiles()
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(grpcURL);
                adminClient = new Admin.AdminClient(channel);
                var reply = await adminClient.GetProfilesAsync(new GetMessage());
                return Ok(reply);
            }
            catch (ArgumentException error)
            {
                return BadRequest(error.Message);
            }
        }

        [HttpPost("perfiles")]
        public async Task<ActionResult> CrearPerfil([FromBody] ProfileMapProtoDTO profileDTO)
        {
            ProfileDTO profile = new ProfileDTO();
            profile.ProfileUserId = profileDTO.Username;
            profile.Descripcion = profileDTO.Descripcion;
            foreach (string habilidad in profileDTO.Habilidades)
            {
                profile.Habilidades.Add(habilidad);
            }

            using var channel = GrpcChannel.ForAddress(grpcURL);
            adminClient = new Admin.AdminClient(channel);
            var reply = await adminClient.PostProfileAsync(profile);
            return Ok(reply.Message);
        }

        [HttpPut("perfiles")]
        public async Task<ActionResult> ModificarPerfil([FromBody] ProfileMapProtoDTO profileDTO)
        {
            ModifyProfileDTO profile = new ModifyProfileDTO();
            profile.ProfileUserId = profileDTO.Username;
            profile.Descripcion = profileDTO.Descripcion;

            foreach (string habilidad in profileDTO.Habilidades)
            {
                profile.Habilidades.Add(habilidad);
            }

            using var channel = GrpcChannel.ForAddress(grpcURL);
            adminClient = new Admin.AdminClient(channel);
            var reply = await adminClient.ModifyProfileAsync(profile);
            return Ok(reply.Message);
        }

        [HttpDelete("perfiles/{id}")]
        public async Task<ActionResult> EliminarPerfil([FromRoute] string id)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(grpcURL);
                adminClient = new Admin.AdminClient(channel);
                var reply = await adminClient.DeleteProfileAsync(new Id { Username = id });
                return Ok(reply.Message);
            }
            catch (ArgumentException error)
            {
                return BadRequest(error.Message);
            }
        }

        [HttpDelete("perfiles/{id}/image")]
        public async Task<ActionResult> EliminarFoto([FromRoute] string id)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            adminClient = new Admin.AdminClient(channel);
            var reply = await adminClient.DeleteImageAsync(new Id { Username = id });
            return Ok(reply.Message);
        }
    }
}

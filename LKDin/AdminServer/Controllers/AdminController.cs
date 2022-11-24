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

        [HttpGet("usuarioss")]
        public async Task<ActionResult> GetUsers()
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            adminClient = new Admin.AdminClient(channel);
            var reply = await adminClient.GetUsersAsync(new GetMessage());
            return Ok(reply);
        }

        [HttpPost("usuarios")]
        public async Task<ActionResult> PostUser([FromBody] UserDTO user)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            adminClient = new Admin.AdminClient(channel);
            var reply = await adminClient.PostUserAsync(user);
            return Ok(reply.Message);
        }

        [HttpDelete("usuarios/{id}")]
        public async Task<ActionResult> DeleteUser([FromRoute] string id)
        {
            using var channel = GrpcChannel.ForAddress(grpcURL);
            adminClient = new Admin.AdminClient(channel);
            var reply = await adminClient.DeleteUserAsync(new Id { Id_ = id });
            return Ok(reply.Message);
        }
    }
}

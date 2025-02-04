using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.Repositories.ApplicationUserRepo;

namespace Ollama_Component.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TEMP_AuthController : ControllerBase
    {
        public IApplicationUserRepository _UserRepos { get; private set; }

        public TEMP_AuthController(IApplicationUserRepository userRepository)
        {
            _UserRepos = userRepository;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserDTO user)
        {
            var userModel = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = user.Name,
                PasswordHash = user.Password
            };

            await _UserRepos.AddAsync(userModel);

            return Ok("User registered successfully");
        }

        [HttpPost("UserInfo")]
        public async Task<IActionResult> UserInfo(UserDTO user)
        {
            var userModel = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = user.Name,
                PasswordHash = user.Password
            };

            await _UserRepos.AddAsync(userModel);

            return Ok("User info processed successfully");
        }


    }

    public class UserDTO
    {
        public string Id{ get; set; }
        public string Name { get; set; }
        public string Password{ get; set; }
    }
}

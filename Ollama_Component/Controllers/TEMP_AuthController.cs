using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.Repositories.ApplicationUserRepo;
using Ollama_DB_layer.UOW;

namespace Ollama_Component.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TEMP_AuthController : ControllerBase
    {
        public IApplicationUserRepository _UserRepos { get; private set; }
        private readonly IUnitOfWork _unitOfWork;

        public TEMP_AuthController(IApplicationUserRepository userRepository, IUnitOfWork _unitOfWork)
        {
            _UserRepos = userRepository;
            _unitOfWork = _unitOfWork;
        }


        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserDTO user)
        {
            var userModel = new ApplicationUser
            {
                Id = user.Id,
                UserName = user.Name,
                PasswordHash = user.Password,
                Prefrences = "None",
            };

            await _UserRepos.AddAsync(userModel);
            await _unitOfWork.SaveChangesAsync();

            return Ok("User registered successfully");
        }


        [HttpPost("UserInfo")]
        public async Task<IActionResult> UserInfo(string Id)
        {
            
            var info = await _UserRepos.GetByIdAsync(Id);

            return Ok(info);
        }
    }

    public class UserDTO
    {
        public string Id{ get; set; }
        public string Name { get; set; }
        public string Password{ get; set; }
    }
}

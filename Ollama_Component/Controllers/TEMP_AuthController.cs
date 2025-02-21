using Microsoft.AspNetCore.Mvc;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.Repositories.ApplicationUserRepo;
using Ollama_DB_layer.UOW;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Ollama_Component.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TEMP_AuthController : ControllerBase
    {
        private readonly IApplicationUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<UserDTO> _userValidator;

        public TEMP_AuthController(IApplicationUserRepository userRepository, IUnitOfWork unitOfWork, IValidator<UserDTO> userValidator)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _userValidator = userValidator;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserDTO user)
        {
            var validationResult = await _userValidator.ValidateAsync(user);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var existingUser = await _userRepository.GetByIdAsync(user.Id);
            if (existingUser != null)
            {
                return Conflict("User with this ID already exists.");
            }

            var userModel = new ApplicationUser
            {
                Id = user.Id,
                UserName = user.Name,
                PasswordHash = user.Password, // Consider hashing this password
                Prefrences = "None"
            };

            await _userRepository.AddAsync(userModel);
            await _unitOfWork.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

        [HttpGet("UserInfo/{id}")]
        public async Task<IActionResult> UserInfo(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("User ID is required.");
            }

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);
        }

        [HttpGet("Users")]
        public async Task<IActionResult> Users()
        {
            var users = await _userRepository.GetAllAsync();
            return Ok(users);
        }

        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("User ID is required.");
            }

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            await _userRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return Ok("User deleted successfully.");
        }
    }

    public class UserDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }

    public class UserDTOValidator : AbstractValidator<UserDTO>
    {
        public UserDTOValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("User ID is required.")
                .Matches("^[a-zA-Z0-9_-]+$").WithMessage("User ID can only contain letters, numbers, underscores, and hyphens.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MinimumLength(3).WithMessage("Name must be at least 3 characters long.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
        }
    }
}

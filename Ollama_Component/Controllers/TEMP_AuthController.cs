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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<RegisterRequest> _userValidator;

        public TEMP_AuthController(IUnitOfWork unitOfWork, IValidator<RegisterRequest> userValidator)
        {
            _unitOfWork = unitOfWork;
            _userValidator = userValidator;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest user)
        {
            // Validate input
            var validationResult = await _userValidator.ValidateAsync(user);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            // Check if user already exists (by email instead of ID)
            var existingUser = await _unitOfWork.ApplicationUserRepo.GetByUserNameAsync(user.UserName);
            if (existingUser != false)
            {
                return Conflict(new
                {
                    Success = false,
                    Message = "A user with this Username already exists."
                });
            }

            var userModel = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(), // Generate a new GUID
                UserName = user.UserName,
                PasswordHash = user.Password,
                Prefrences = "None"
            };

            // Save user
            await _unitOfWork.ApplicationUserRepo.AddAsync(userModel);
            await _unitOfWork.SaveChangesAsync();

            // Return proper JSON response
            return CreatedAtAction(nameof(Register), new
            {
                Success = true,
                Message = "User registered successfully.",
                UserId = userModel.Id
            });
        }

        [HttpGet("UserInfo/{id}")]
        public async Task<IActionResult> UserInfo(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("User ID is required.");
            }

            var user = await _unitOfWork.ApplicationUserRepo.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);
        }


        [HttpGet("Users")]
        public async Task<IActionResult> Users()
        {
            var users = await _unitOfWork.ApplicationUserRepo.GetAllAsync();
            return Ok(users);
        }


        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("User ID is required.");
            }

            var user = await _unitOfWork.ApplicationUserRepo.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            await _unitOfWork.ApplicationUserRepo.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return Ok("User deleted successfully.");
        }
    }

    public class RegisterRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Name is required.")
                .MinimumLength(3).WithMessage("Name must be at least 3 characters long.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
        }
    }
}

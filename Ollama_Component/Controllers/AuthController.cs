using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ollama_Component.Services.AuthService.Models;
using Ollama_Component.Services.AuthService;

namespace Ollama_Component.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }



        // Register Endpoint 

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterUserAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            return Ok(result);
        }




        // Login Endpoint

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] TokenRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginUserAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            return Ok(result);
        }




        // Update Profile Endpoint 
        [HttpPost("updateprofile")]
        public async Task<IActionResult> UpdateProfileAsync([FromBody] UpdateProfileModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.UpdateProfileAsync(model);
            if (!string.IsNullOrEmpty(result))
                return BadRequest(result);

            return Ok("Profile updated successfully");
        }





        // Change Password Endpoint
        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ChangePasswordAsync(model);
            if (!string.IsNullOrEmpty(result))
                return BadRequest(result);

            return Ok("Password changed successfully");
        }





        // Forgot Password Endpoint 
        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] string email)
        {
            var result = await _authService.ForgotPasswordAsync(email);
            if (result == null)
            {
                return BadRequest("User not found");
            }
            return Ok(result);
        }



        // Reset Password Endpoint
        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResetPasswordAsync(model);
            if (!string.IsNullOrEmpty(result))
                return BadRequest(result);

            return Ok("Password reset successfully");
        }



        // Assign Role Endpoint

        [HttpPost("assignrole")]
        public async Task<IActionResult> AssignRoleAsync([FromBody] RoleModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.AssignRoleAsync(model);

            if (!string.IsNullOrEmpty(result))
                return BadRequest(result);

            return Ok(model);
        }



        // Disassign Role Endpoint

        [HttpPost("deassignrole")]
        public async Task<IActionResult> DeassignRoleAsync([FromBody] RoleModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.DeassignRoleAsync(model);
            if (!string.IsNullOrEmpty(result))
                return BadRequest(result);

            return Ok("Role deassigned successfully");
        }

    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ollama_Component.Services.AuthService;
using Ollama_Component.Services.AuthService.Models;
using System.ComponentModel.DataAnnotations;

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
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return BadRequest("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Username))
            {
                return BadRequest("Username is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest("Password is required.");
            }

            var result = await _authService.RegisterUserAsync(model);

            if (!result.IsAuthenticated)
            {
                return BadRequest(result.Message);
            }

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(result);
        }



        // Login Endpoint
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] TokenRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return BadRequest("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest("Password is required.");
            }

            var result = await _authService.LoginUserAsync(model);

            if (!result.IsAuthenticated)
            {
                return BadRequest(result.Message);
            }

            if (!string.IsNullOrEmpty(result.RefreshToken))
                SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(result);
        }



        // Update Profile Endpoint (Modified)
        [HttpPost("updateprofile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfileAsync([FromBody] UpdateProfileModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return BadRequest("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Username))
            {
                return BadRequest("Username is required.");
            }

            string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var result = await _authService.UpdateProfileAsync(model, token);

            if (!string.IsNullOrEmpty(result))
            {
                return BadRequest(result);
            }

            return Ok("Profile updated successfully");
        }



        // Change Password Endpoint (Modified)
        [HttpPost("changepassword")]
        [Authorize]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(model.OldPassword))
            {
                return BadRequest("Old password is required.");
            }

            if (string.IsNullOrWhiteSpace(model.NewPassword))
            {
                return BadRequest("New password is required.");
            }

            string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var result = await _authService.ChangePasswordAsync(model, token);

            if (!string.IsNullOrEmpty(result))
            {
                return BadRequest(result);
            }

            return Ok("Password changed successfully");
        }



        // Forgot Password Endpoint
        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequestModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return BadRequest("Email is required.");
            }

            if (!new EmailAddressAttribute().IsValid(model.Email))
            {
                return BadRequest("Invalid email format.");
            }

            var result = await _authService.ForgotPasswordAsync(model);

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
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return BadRequest("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Token))
            {
                return BadRequest("Token is required.");
            }

            if (string.IsNullOrWhiteSpace(model.NewPassword))
            {
                return BadRequest("New password is required.");
            }

            var result = await _authService.ResetPasswordAsync(model);

            if (!string.IsNullOrEmpty(result))
            {
                return BadRequest(result);
            }

            return Ok("Password reset successfully");
        }



        // Assign Role Endpoint
        [HttpPost("assignrole")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRoleAsync([FromBody] RoleModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(model.UserId))
            {
                return BadRequest("UserId is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Role))
            {
                return BadRequest("Role is required.");
            }

            var result = await _authService.AssignRoleAsync(model);

            if (!string.IsNullOrEmpty(result))
            {
                return BadRequest(result);
            }

            return Ok(model);
        }



        // Disassign Role Endpoint
        [HttpPost("deassignrole")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeassignRoleAsync([FromBody] RoleModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(model.UserId))
            {
                return BadRequest("UserId is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Role))
            {
                return BadRequest("Role is required.");
            }

            var result = await _authService.DeassignRoleAsync(model);

            if (!string.IsNullOrEmpty(result))
            {
                return BadRequest(result);
            }

            return Ok("Role deassigned successfully");
        }



        //refreshtoken endpoint
        [HttpGet("RefreshToken")]
        public async Task<IActionResult> RefreshTokenAsync()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var result = await _authService.RefreshTokenAsync(refreshToken);

            if (!result.IsAuthenticated)
                return BadRequest(result);

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(result);
        }



        //logout
        [HttpPost("logout")]
        public async Task<IActionResult> LoggoutAsync([FromBody] logout model)
        {
            var token = model.RefreshToken ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is required!");

            var result = await _authService.LoggoutAsync(token);

            if (!result)
                return BadRequest("Token is invalid!");

            return Ok(new { message = "Logged out successfully." });
        }




        //getrole
        [HttpGet("getroles/{userId}")]
        public async Task<IActionResult> GetRolesAsync(string userId)
        {
            var roles = await _authService.GetRolesAsync(userId);
            return Ok(roles);
        }




        //retrive refreshtoken cookeied
        private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires.ToLocalTime(),
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }



    }
}
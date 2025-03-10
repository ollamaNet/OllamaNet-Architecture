using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ollama_Component.Services.AuthService.Models;
using Ollama_Component.Services.AuthService;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

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

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(result);
        }




        // Login Endpoint

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] TokenRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginUserAsync(model);


            if (!string.IsNullOrEmpty(result.RefreshToken))
                SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

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


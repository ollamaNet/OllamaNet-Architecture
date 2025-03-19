using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Ollama_DB_layer.Entities;
using Ollama_Component.Services.AuthService.Helpers;
using Ollama_Component.Services.AuthService.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using Microsoft.EntityFrameworkCore;
using Ollama_DB_layer.Persistence.Migrations;

namespace Ollama_Component.Services.AuthService
{
    public class AuthService : IAuthService
    {


        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWTManager _jwtManager;

        public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, JWTManager jwtManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtManager = jwtManager;
        }


        // Get user by token
        public async Task<ApplicationUser> GetUserByTokenAsync(string token)
        {
            var claimsPrincipal = _jwtManager.ValidateToken(token);
            if (claimsPrincipal == null)
            {
                Console.WriteLine("Token validation failed.");
                return null;
            }

            var userIdClaim = claimsPrincipal.FindFirst("UserId");
            if (userIdClaim == null)
            {
                Console.WriteLine("User ID claim not found in token.");
                return null;
            }

            Console.WriteLine($"Extracted User ID: {userIdClaim.Value}");
            return await _userManager.FindByIdAsync(userIdClaim.Value);
        }


        // Register user service
        public async Task<AuthModel> RegisterUserAsync(RegisterModel model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message = "Email is already registered!" };

            if (await _userManager.FindByNameAsync(model.Username) is not null)
                return new AuthModel { Message = "Username is already registered!" };

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthModel { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, "User");
            var jwtSecurityToken = await _jwtManager.CreateJwtToken(user, _userManager);

            //refreshtoken

            var refreshTokenre = _jwtManager.GenerateRefreshToken();

            user.RefreshTokens.Add(refreshTokenre);

            await _userManager.UpdateAsync(user);


            return new AuthModel
            {
                Email = user.Email,
                ExpiresOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                Roles = new List<string> { "User" },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Username = user.UserName,
                 RefreshToken = refreshTokenre.Token,
                RefreshTokenExpiration = refreshTokenre.ExpiresOn,

            };
        }



        // Login service
        public async Task<AuthModel> LoginUserAsync(TokenRequestModel model)
        {
            var authModel = new AuthModel();
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authModel.Message = "Email or Password is incorrect!";
                return authModel;
            }

            var jwtSecurityToken = await _jwtManager.CreateJwtToken(user, _userManager);
            var rolesList = await _userManager.GetRolesAsync(user);

            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.Email = user.Email;
            authModel.Username = user.UserName;
            authModel.ExpiresOn = jwtSecurityToken.ValidTo;
            authModel.Roles = rolesList.ToList();

                //refreshtoken
            if (user.RefreshTokens.Any(t => t.IsActive))
            {
                var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
                authModel.RefreshToken = activeRefreshToken.Token;
                authModel.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
            }
            else
            {
                var refreshToken = _jwtManager.GenerateRefreshToken();
                authModel.RefreshToken = refreshToken.Token;
                authModel.RefreshTokenExpiration = refreshToken.ExpiresOn;
                user.RefreshTokens.Add(refreshToken);
                await _userManager.UpdateAsync(user);
            }



            return authModel;
        }



        // Update Profile service
        public async Task<string> UpdateProfileAsync(UpdateProfileModel model, string token)
        {
            var user = await GetUserByTokenAsync(token);

            if (user == null)
            {
                return "User not found";
            }

            user.Email = model.Email;
            user.UserName = model.Username;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded ? "" : "Something went wrong";
        }




        // Change Password service
        public async Task<string> ChangePasswordAsync(ChangePasswordModel model, string token)
        {
            var user = await GetUserByTokenAsync(token);

            if (user == null)
            {
                return "User not found";
            }

            // Verify the old password before attempting to change it
            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.OldPassword);
            if (!passwordCheck)
                return "Incorrect old password";

            // Prevent changing to the same password
            if (model.OldPassword == model.NewPassword)
                return "New password must be different from the old password";

            // Attempt to change the password
            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (result.Succeeded)
                return "";

            // Check for specific password policy errors
            if (result.Errors.Any())
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return $"Password change failed: {errors}";
            }

            return "Password change failed due to an unknown error";
        }



        // Forgot Password service
        public async Task<ForgotPasswordResponseModel> ForgotPasswordAsync(ForgotPasswordRequestModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
                return null; // Or throw an exception, depending on your error handling strategy

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(token);

            return new ForgotPasswordResponseModel
            {
                Token = encodedToken,
                ResetPasswordLink = $"https://localhost:7006/resetpassword?token={encodedToken}"
            };
        }



        // Reset Password service
        public async Task<string> ResetPasswordAsync(ResetPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
                return "Error: User with this email does not exist.";

            // Check if the new password is the same as the old one
            var isSamePassword = await _userManager.CheckPasswordAsync(user, model.NewPassword);
            if (isSamePassword)
                return "Error: You cannot reuse your old password. Please choose a new password.";

            var decodedToken = HttpUtility.UrlDecode(model.Token);
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (result.Succeeded)
                return "";

            var errors = result.Errors.Select(e => e.Description).ToList();

            if (errors.Any(e => e.Contains("Invalid token")))
                return "Error: Invalid or expired reset token.";

            if (errors.Any(e => e.Contains("password")))
                return "Error: New password does not meet security requirements.";

            return "Password reset failed: " + string.Join(" | ", errors);
        }



         // Assign Role service
        public async Task<string> AssignRoleAsync(RoleModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user is null || !await _roleManager.RoleExistsAsync(model.Role))
                return "Invalid user ID or Role";

            if (await _userManager.IsInRoleAsync(user, model.Role))
                return "User already assigned to this role";

            var result = await _userManager.AddToRoleAsync(user, model.Role);
            return result.Succeeded ? "" : "Something went wrong";
        }


        // Disassign Role service
        public async Task<string> DeassignRoleAsync(RoleModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user is null || !await _roleManager.RoleExistsAsync(model.Role))
                return "Invalid user ID or Role";

            if (!await _userManager.IsInRoleAsync(user, model.Role))
                return "User is not assigned to this role";

            var result = await _userManager.RemoveFromRoleAsync(user, model.Role);
            return result.Succeeded ? "" : "Something went wrong";
        }




          //refreshtoken service
        public async Task<AuthModel> RefreshTokenAsync(string refreshtoken)
        {
            var authModel = new AuthModel();

              var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshtoken));


            if (user == null)
            {
                // authModel.IsAuthenticated = false;
                authModel.Message = "invalid token";
                return authModel;

            }

            var rtoken = user.RefreshTokens.Single(t => t.Token == refreshtoken);

            if (!rtoken.IsActive)
            {
                // authModel.IsAuthenticated = false;
                authModel.Message = "inactive token";
                return authModel;

            }

            rtoken.RevokedOn = DateTime.UtcNow;

            //  Increment TokenVersion (invalidates old access tokens)
            user.TokenVersion++;

            var NewRToken = _jwtManager.GenerateRefreshToken();

            user.RefreshTokens.Add(NewRToken);

            await _userManager.UpdateAsync(user);

            var NewJwtToken = await _jwtManager.CreateJwtToken(user, _userManager);

            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(NewJwtToken);
           // authModel.Email = user.Email;
           // authModel.Username = user.UserName;
          //  var roles = await _userManager.GetRolesAsync(user);
          // authModel.Roles = roles.ToList();
            authModel.RefreshToken = NewRToken.Token;
            authModel.RefreshTokenExpiration = NewRToken.ExpiresOn;

            return authModel;


        }



        //logout service
        public async Task<bool> LoggoutAsync(string refreshtoken)
        {
            var authModel = new AuthModel();

            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshtoken));
            if (user == null)
            {
                return false;

            }

            var rtoken = user.RefreshTokens.Single(t => t.Token == refreshtoken);

            if (!rtoken.IsActive)
            {
                return false;
            }

            rtoken.RevokedOn = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            return true;

        }




           //get role service
          public async Task<List<string>> GetRolesAsync(string userId)
          {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return new List<string>();

            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
          }



    }
}
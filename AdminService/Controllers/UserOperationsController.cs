using AdminService.Services.UserOperations;
using AdminService.Services.UserOperations.DTOs;
using AdminService.Controllers.Validators;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace AdminService.Controllers
{
    //[Authorize("Admin")] // Commented for testing
    [Route("api/admin/users")]
    [ApiController]
    public class UserOperationsController : ControllerBase
    {
        private readonly IUserOperationsService _userAdminService;
        private readonly CreateUserRequestValidator _createUserValidator;
        private readonly UpdateUserProfileRequestValidator _updateProfileValidator;
        private readonly ChangeUserRoleRequestValidator _changeRoleValidator;
        private readonly ToggleUserStatusRequestValidator _toggleStatusValidator;
        private readonly ResetPasswordRequestValidator _resetPasswordValidator;
        private readonly LockUserRequestValidator _lockUserValidator;

        public UserOperationsController(
            IUserOperationsService userAdminService,
            CreateUserRequestValidator createUserValidator,
            UpdateUserProfileRequestValidator updateProfileValidator,
            ChangeUserRoleRequestValidator changeRoleValidator,
            ToggleUserStatusRequestValidator toggleStatusValidator,
            ResetPasswordRequestValidator resetPasswordValidator,
            LockUserRequestValidator lockUserValidator)
        {
            _userAdminService = userAdminService;
            _createUserValidator = createUserValidator;
            _updateProfileValidator = updateProfileValidator;
            _changeRoleValidator = changeRoleValidator;
            _toggleStatusValidator = toggleStatusValidator;
            _resetPasswordValidator = resetPasswordValidator;
            _lockUserValidator = lockUserValidator;
        }



        // GET: api/admin/users
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? role = null)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("Invalid pagination parameters. Page number and page size must be greater than 0.");

            var (users, totalCount) = await _userAdminService.GetUsersPaginatedAsync(pageNumber, pageSize, role);
            
            return Ok(new
            {
                Users = users,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }





        // GET: api/admin/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("User ID cannot be empty.");

            try
            {
                var user = await _userAdminService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }




        // GET: api/admin/users/email/{email}
        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email cannot be empty.");

            try
            {
                var user = await _userAdminService.GetUserByEmailAsync(email);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }



        // GET: api/admin/users/search
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery][Required] string searchTerm, [FromQuery] string? role = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term cannot be empty.");

            var users = await _userAdminService.SearchUsersAsync(searchTerm, role);
            return Ok(users);
        }



        // POST: api/admin/users
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            ValidationResult validationResult = await _createUserValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { Property = e.PropertyName, Error = e.ErrorMessage }));
            }

            try
            {
                await _userAdminService.CreateUserAsync(request);
                return StatusCode(201, new { Message = "User created successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }



        // PUT: api/admin/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserProfileRequest request)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("User ID cannot be empty.");

            ValidationResult validationResult = await _updateProfileValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { Property = e.PropertyName, Error = e.ErrorMessage }));
            }

            try
            {
                await _userAdminService.UpdateUserProfileAsync(id, request);
                return Ok(new { Message = "User updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }



        // PATCH: api/admin/users/{id}/role
        [HttpPatch("{id}/role")]
        public async Task<IActionResult> ChangeUserRole(string id, [FromBody] string newRole)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("User ID cannot be empty.");

            if (string.IsNullOrWhiteSpace(newRole))
                return BadRequest("New role cannot be empty.");

            var request = new ChangeUserRoleRequest { UserId = id, NewRole = newRole };
            ValidationResult validationResult = await _changeRoleValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { Property = e.PropertyName, Error = e.ErrorMessage }));
            }

            try
            {
                await _userAdminService.ChangeUserRoleAsync(id, newRole);
                return Ok(new { Message = $"User role changed to {newRole}" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }





        // PATCH: api/admin/users/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ToggleUserStatus(string id, [FromBody] bool isActive)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("User ID cannot be empty.");

            var request = new ToggleUserStatusRequest { UserId = id, IsActive = isActive };
            ValidationResult validationResult = await _toggleStatusValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { Property = e.PropertyName, Error = e.ErrorMessage }));
            }

            try
            {
                await _userAdminService.ToggleUserStatusAsync(id, isActive);
                return Ok(new { Message = $"User status is now {(isActive ? "active" : "inactive")}" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }





        // DELETE: api/admin/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("User ID cannot be empty.");

            try
            {
                await _userAdminService.DeleteUserAsync(id);
                return Ok(new { Message = "User deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }




        // PATCH: api/admin/users/{id}/soft-delete
        [HttpPatch("{id}/soft-delete")]
        public async Task<IActionResult> SoftDeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("User ID cannot be empty.");

            try
            {
                await _userAdminService.SoftDeleteUserAsync(id);
                return Ok(new { Message = "User deactivated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }




        // POST: api/admin/users/{id}/reset-password
        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(string id, [FromBody] string newPassword)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("User ID cannot be empty.");

            var request = new ResetPasswordRequest { UserId = id, NewPassword = newPassword };
            ValidationResult validationResult = await _resetPasswordValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { Property = e.PropertyName, Error = e.ErrorMessage }));
            }

            try
            {
                await _userAdminService.ResetUserPasswordAsync(id, newPassword);
                return Ok(new { Message = "Password reset successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }





        // POST: api/admin/users/{id}/lock
        [HttpPost("{id}/lock")]
        public async Task<IActionResult> LockUser(string id, [FromBody] int lockoutMinutes = 30)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("User ID cannot be empty.");

            var request = new LockUserRequest { UserId = id, LockoutMinutes = lockoutMinutes };
            ValidationResult validationResult = await _lockUserValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { Property = e.PropertyName, Error = e.ErrorMessage }));
            }

            try
            {
                var duration = TimeSpan.FromMinutes(lockoutMinutes);
                await _userAdminService.LockUserAccountAsync(id, duration);
                return Ok(new { Message = $"User locked for {lockoutMinutes} minutes" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }




        // POST: api/admin/users/{id}/unlock
        [HttpPost("{id}/unlock")]
        public async Task<IActionResult> UnlockUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("User ID cannot be empty.");

            try
            {
                await _userAdminService.UnlockUserAccountAsync(id);
                return Ok(new { Message = "User unlocked successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
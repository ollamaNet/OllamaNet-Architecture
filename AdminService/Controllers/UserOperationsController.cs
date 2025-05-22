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
    [Route("api/Admin/[controller]")]
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
        private readonly CreateRoleRequestValidator _createRoleValidator;

        public UserOperationsController(
            IUserOperationsService userAdminService,
            CreateUserRequestValidator createUserValidator,
            UpdateUserProfileRequestValidator updateProfileValidator,
            ChangeUserRoleRequestValidator changeRoleValidator,
            ToggleUserStatusRequestValidator toggleStatusValidator,
            ResetPasswordRequestValidator resetPasswordValidator,
            LockUserRequestValidator lockUserValidator,
            CreateRoleRequestValidator createRoleValidator)
        {
            _userAdminService = userAdminService;
            _createUserValidator = createUserValidator;
            _updateProfileValidator = updateProfileValidator;
            _changeRoleValidator = changeRoleValidator;
            _toggleStatusValidator = toggleStatusValidator;
            _resetPasswordValidator = resetPasswordValidator;
            _lockUserValidator = lockUserValidator;
            _createRoleValidator = createRoleValidator;
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





        // GET: api/admin/users/search
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery][Required] string searchTerm, [FromQuery] string? role = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term cannot be empty.");

            var users = await _userAdminService.SearchUsersAsync(searchTerm, role);
            return Ok(users);
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




        // GET: api/admin/users/roles
        [HttpGet("roles")]
        public async Task<IActionResult> GetAvailableRoles()
        {
            try
            {
                var roles = await _userAdminService.GetAvailableRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving roles", Error = ex.Message });
            }
        }




        // POST: api/admin/users/roles
        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            // Validate request
            ValidationResult validationResult = await _createRoleValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { Property = e.PropertyName, Error = e.ErrorMessage }));
            }

            try
            {
                var createdRole = await _userAdminService.CreateRoleAsync(request);
                return CreatedAtAction(nameof(GetAvailableRoles), createdRole);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error creating role", Error = ex.Message });
            }
        }




        // DELETE: api/admin/users/roles/{id}
        [HttpDelete("roles/{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Role ID cannot be empty.");

            try
            {
                await _userAdminService.DeleteRoleAsync(id);
                return Ok(new { Message = "Role deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error deleting role", Error = ex.Message });
            }
        }
    }
}
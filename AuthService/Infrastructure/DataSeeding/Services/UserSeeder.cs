using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Ollama_DB_layer.Entities;
using AuthService.DataSeeding.Interfaces;
using AuthService.DataSeeding.Options;

namespace AuthService.DataSeeding.Services
{
    public class UserSeeder : IUserSeeder
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserSeeder> _logger;

        public UserSeeder(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserSeeder> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedAdminUserAsync(AdminAccountOptions options)
        {
            var adminUser = await _userManager.FindByEmailAsync(options.Email);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = options.Username,
                    Email = options.Email,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(adminUser, options.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Admin user '{options.Email}' created successfully.");
                }
                else
                {
                    _logger.LogError($"Failed to create admin user '{options.Email}': {string.Join(", ", result.Errors)}");
                    return;
                }
            }
            else
            {
                _logger.LogInformation($"Admin user '{options.Email}' already exists.");
            }

            // Ensure admin user has the Admin role
            if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                var roleResult = await _userManager.AddToRoleAsync(adminUser, "Admin");
                if (roleResult.Succeeded)
                {
                    _logger.LogInformation($"Admin user '{options.Email}' assigned to 'Admin' role.");
                }
                else
                {
                    _logger.LogError($"Failed to assign 'Admin' role to user '{options.Email}': {string.Join(", ", roleResult.Errors)}");
                }
            }
            else
            {
                _logger.LogInformation($"Admin user '{options.Email}' is already in 'Admin' role.");
            }
        }
    }
} 
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using AuthService.DataSeeding.Interfaces;
using AuthService.DataSeeding.Options;

namespace AuthService.DataSeeding.Services
{
    public class DataSeeder : IDataSeeder
    {
        private readonly IRoleSeeder _roleSeeder;
        private readonly IUserSeeder _userSeeder;
        private readonly DataSeedingOptions _options;
        private readonly ILogger<DataSeeder> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public DataSeeder(
            IRoleSeeder roleSeeder,
            IUserSeeder userSeeder,
            IOptions<DataSeedingOptions> options,
            ILogger<DataSeeder> logger)
        {
            _roleSeeder = roleSeeder;
            _userSeeder = userSeeder;
            _options = options.Value;
            _logger = logger;

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    _options.RetryPolicy?.MaxRetries ?? 3,
                    retryAttempt => TimeSpan.FromSeconds(_options.RetryPolicy?.DelayInSeconds ?? 2),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Seeding attempt {retryCount} failed. Waiting {timeSpan.TotalSeconds} seconds before next retry. Exception: {exception.Message}");
                    });
        }

        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting data seeding process...");
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _roleSeeder.SeedRolesAsync(_options.Roles);
                await _userSeeder.SeedAdminUserAsync(_options.AdminAccount);
            });
            _logger.LogInformation("Data seeding process completed.");
        }

        public Task<bool> ShouldSeedAsync()
        {
            // Optionally implement logic to determine if seeding is needed
            // For now, always return true
            return Task.FromResult(true);
        }
    }
} 
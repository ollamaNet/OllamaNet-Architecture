using System.Collections.Generic;

namespace AuthService.DataSeeding.Options
{
    public class DataSeedingOptions
    {
        public List<string> Roles { get; set; }
        public AdminAccountOptions AdminAccount { get; set; }
        public RetryPolicyOptions RetryPolicy { get; set; }
    }

    public class AdminAccountOptions
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RetryPolicyOptions
    {
        public int MaxRetries { get; set; }
        public int DelayInSeconds { get; set; }
    }
} 
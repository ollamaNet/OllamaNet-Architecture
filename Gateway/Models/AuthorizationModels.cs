namespace Gateway.Models
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
        // Add more roles as needed
    }

    public class EndpointAuthorizationRequirement
    {
        public string[] AllowedRoles { get; set; }
    }
} 
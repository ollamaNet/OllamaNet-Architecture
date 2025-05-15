namespace AdminService.Services.UserOperations.DTOs
{
    /// <summary>
    /// Data transfer object for role information
    /// </summary>
    public class RoleResponse
    {
        /// <summary>
        /// Role identifier
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// Role name
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Role description (optional)
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Normalized role name (for internal use)
        /// </summary>
        public string NormalizedName { get; set; } = string.Empty;
    }
}
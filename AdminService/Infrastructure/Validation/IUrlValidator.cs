namespace AdminService.Infrastructure.Validation
{
    /// <summary>
    /// Interface for URL validation
    /// </summary>
    public interface IUrlValidator
    {
        /// <summary>
        /// Validates if a URL is in a valid format
        /// </summary>
        /// <param name="url">The URL to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        bool IsValidUrl(string url);
    }
} 
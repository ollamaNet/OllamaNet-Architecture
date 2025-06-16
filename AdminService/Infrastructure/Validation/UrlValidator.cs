using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace AdminService.Infrastructure.Validation
{
    /// <summary>
    /// Implementation of IUrlValidator for validating URLs
    /// </summary>
    public class UrlValidator : IUrlValidator
    {
        private readonly ILogger<UrlValidator> _logger;
        private static readonly Regex UrlRegex = new Regex(
            @"^(http|https)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Constructor for UrlValidator
        /// </summary>
        /// <param name="logger">Logger</param>
        public UrlValidator(ILogger<UrlValidator> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogWarning("URL validation failed: URL is null or empty");
                return false;
            }

            try
            {
                // Check if the URL matches the regex pattern
                if (!UrlRegex.IsMatch(url))
                {
                    _logger.LogWarning("URL validation failed: URL format is invalid for {Url}", url);
                    return false;
                }

                // Additional validation: Check if the URL can be parsed as a URI
                var uri = new Uri(url);
                
                // Check if the scheme is http or https
                if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                {
                    _logger.LogWarning("URL validation failed: URL scheme is not HTTP or HTTPS for {Url}", url);
                    return false;
                }

                _logger.LogDebug("URL validation passed for {Url}", url);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "URL validation failed with exception for {Url}", url);
                return false;
            }
        }
    }
}
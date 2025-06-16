using System;
using System.Threading.Tasks;

namespace AdminService.Infrastructure.Configuration
{
    /// <summary>
    /// Interface for managing the Inference Engine configuration
    /// </summary>
    public interface IInferenceEngineConfiguration
    {
        /// <summary>
        /// Gets the current base URL for the Inference Engine
        /// </summary>
        /// <returns>The current base URL</returns>
        string GetBaseUrl();
        /// <summary>
        /// Updates the base URL for the Inference Engine
        /// </summary>
        /// <param name="newUrl">The new URL to use</param>
        Task UpdateBaseUrl(string newUrl);
        /// <summary>
        /// Event raised when the base URL changes
        /// </summary>
        event Action<string> BaseUrlChanged;
    }
} 
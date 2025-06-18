using System;

namespace AdminService.Infrastructure.Messaging.Models
{
    /// <summary>
    /// Message schema for Inference Engine URL updates
    /// </summary>
    public class InferenceUrlUpdateMessage
    {
        /// <summary>
        /// The new URL for the Inference Engine
        /// </summary>
        public string NewUrl { get; set; }
        
        /// <summary>
        /// Timestamp of when the message was created
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Identifier for the service
        /// </summary>
        public string ServiceId { get; set; } = "inference-engine";
        
        /// <summary>
        /// Message schema version
        /// </summary>
        public string Version { get; set; } = "1.0";
    }
} 
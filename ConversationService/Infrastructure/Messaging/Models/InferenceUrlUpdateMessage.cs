using System;

namespace ConversationService.Infrastructure.Messaging.Models;

public class InferenceUrlUpdateMessage
{
    public string NewUrl { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string ServiceId { get; set; } = "inference-engine";
    public string Version { get; set; } = "1.0";
} 
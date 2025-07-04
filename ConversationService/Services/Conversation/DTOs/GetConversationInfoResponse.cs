﻿using Microsoft.Identity.Client;
using System;

namespace ConversationServices.Services.ConversationService.DTOs
{
    public class GetConversationInfoResponse
    {
        public string ConversationId { get; set; }
        public string FolderId { get; set; }
        public string Title { get; set; }
        public string ModelName { get; set; }
        public string SystemMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TokenUsage { get; set; }
    }
}
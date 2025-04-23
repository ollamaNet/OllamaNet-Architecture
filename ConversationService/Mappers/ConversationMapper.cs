using ConversationService.ConversationService.DTOs;
using Ollama_DB_layer.Entities;
using System;

namespace ConversationService.Mappers
{
    /// <summary>
    /// Static mapper class that handles conversion between Conversation entities and DTOs.
    /// </summary>
    public static class ConversationMapper
    {
        /// <summary>
        /// Maps a Conversation entity to a GetConversationInfoResponse DTO.
        /// </summary>
        /// <param name="conversation">The conversation entity to map</param>
        /// <returns>A GetConversationInfoResponse DTO</returns>
        public static GetConversationInfoResponse ToConversationInfoResponse(Conversation conversation)
        {
            if (conversation == null)
                throw new ArgumentNullException(nameof(conversation));
                
            return new GetConversationInfoResponse
            {
                ConversationId = conversation.Id,
                Title = conversation.Title,
                ModelName = conversation.AI_Id,
                SystemMessage = conversation.SystemMessage,
                CreatedAt = conversation.CreatedAt,
                TokenUsage = conversation.TokensUsed.ToString()
            };
        }
        
        /// <summary>
        /// Maps a Conversation entity to an OpenConversationResponse DTO.
        /// </summary>
        /// <param name="conversation">The conversation entity to map</param>
        /// <returns>An OpenConversationResponse DTO</returns>
        public static OpenConversationResponse ToOpenConversationResponse(Conversation conversation)
        {
            if (conversation == null)
                throw new ArgumentNullException(nameof(conversation));
                
            return new OpenConversationResponse
            {
                ConversationId = conversation.Id,
                Title = conversation.Title,
                Modelname = conversation.AI_Id
            };
        }

        /// <summary>
        /// Maps an OpenConversationRequest DTO to a Conversation entity.
        /// </summary>
        /// <param name="request">The request DTO to map</param>
        /// <param name="userId">The user ID to associate with the conversation</param>
        /// <returns>A Conversation entity</returns>
        public static Conversation ToConversationEntity(OpenConversationRequest request, string userId)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
                
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
                
            return new Conversation
            {
                Title = request.Title,
                User_Id = userId,
                AI_Id = request.ModelName,
                SystemMessage = request.SystemMessage,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
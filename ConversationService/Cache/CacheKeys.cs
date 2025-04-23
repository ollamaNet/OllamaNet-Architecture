namespace ConversationService.Cache;

public static class CacheKeys
{
    // Conversation-related cache keys
    public const string ConversationList = "conversations:user:{0}"; // {0} = userId
    public const string ConversationListPaginated = "conversations:user:{0}:page:{1}:size:{2}"; // {0} = userId, {1} = page, {2} = size
    public const string ConversationSearch = "conversations:user:{0}:search:{1}:page:{2}:size:{3}"; // {0} = userId, {1} = searchTerm, {2} = page, {3} = size
    public const string ConversationInfo = "conversation:info:{0}"; // {0} = conversationId
    public const string ConversationMessages = "conversation:messages:{0}"; // {0} = conversationId
    public const string ConversationLatestMessages = "conversation:messages:{0}:latest:{1}"; // {0} = conversationId, {1} = count
}
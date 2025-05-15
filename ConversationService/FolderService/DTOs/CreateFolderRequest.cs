namespace ConversationService.FolderService.DTOs
{
    public class CreateFolderRequest
    {
        public string Name { get; set; }
        public string? RootFolderId { get; set; }
    }
} 
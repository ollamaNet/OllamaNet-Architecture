namespace ConversationService.NoteService.DTOs
{
    public class UpdateNoteRequest
    {
        public string Content { get; set; }
        public int? FromIndex { get; set; }
        public int? ToIndex { get; set; }
    }
}
namespace ConversationServices.Services.NoteService.DTOs
{
    public class AddNoteRequest
    {
        public string ResponseId { get; set; }
        public string Content { get; set; }
        public int FromIndex { get; set; }
        public int ToIndex { get; set; }
    }
}
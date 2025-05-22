using Ollama_DB_layer.Entities;
using System;

namespace ConversationService.NoteService.DTOs
{
    public class NoteResponse
    {
        public string Id { get; set; }
        public string Response_Id { get; set; }
        public string Content { get; set; }
        public int FromIndex { get; set; }
        public int ToIndex { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public static NoteResponse FromEntity(Note note)
        {
            return new NoteResponse
            {
                Id = note.Id,
                Response_Id = note.Response_Id,
                Content = note.Content,
                FromIndex = note.FromIndex,
                ToIndex = note.ToIndex,
                CreatedAt = note.CreatedAt,
                IsDeleted = note.IsDeleted
            };
        }
    }
}
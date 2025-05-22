using Ollama_DB_layer.Entities;
using ConversationService.NoteService.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConversationService.NoteService
{
    public interface INoteService
    {
        // Add note
        Task<Note> AddNoteAsync(AddNoteRequest request);
        
        // Delete note (hard delete)
        Task<bool> DeleteNoteAsync(string noteId, string responseId);
        
        // Soft delete note
        Task<bool> SoftDeleteNoteAsync(string noteId, string responseId);
        
        // Update note
        Task<Note> UpdateNoteAsync(string noteId, string responseId, UpdateNoteRequest request);
        
        // Get note by note id and response id
        Task<Note> GetNoteAsync(string noteId, string responseId);
        
        // Get all notes by response id
        Task<IEnumerable<Note>> GetNotesByResponseIdAsync(string responseId);
        
        // Get all notes of responses in a conversation
        Task<IEnumerable<Note>> GetNotesInConversationAsync(string conversationId);
    }
}

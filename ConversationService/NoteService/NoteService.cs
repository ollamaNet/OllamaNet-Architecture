using Microsoft.Extensions.Logging;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.UOW;
using ConversationService.NoteService.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConversationService.NoteService
{
    public class NoteService : INoteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NoteService> _logger;

        public NoteService(IUnitOfWork unitOfWork, ILogger<NoteService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Note> AddNoteAsync(AddNoteRequest request)
        {
            try
            {
                var note = new Note
                {
                    Response_Id = request.ResponseId,
                    Content = request.Content,
                    FromIndex = request.FromIndex,
                    ToIndex = request.ToIndex,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _unitOfWork.NoteRepo.AddAsync(note);
                await _unitOfWork.SaveChangesAsync();
                return note;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding note for response {ResponseId}", request.ResponseId);
                throw;
            }
        }


        public async Task<bool> DeleteNoteAsync(string responseId, string noteId)
        {
            try
            {
                await _unitOfWork.NoteRepo.DeleteAsync(responseId, noteId);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting note {ResponseId} for response {NoteId}", responseId, noteId);
                throw;
            }
        }

        public async Task<bool> SoftDeleteNoteAsync(string responseId, string noteId)
        {
            try
            {
                
                await _unitOfWork.NoteRepo.SoftDeleteAsync(responseId, noteId);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting note {NoteId} for response {ResponseId}", responseId, noteId);
                throw;
            }
        }

        public async Task<Note> UpdateNoteAsync(string noteId, string responseId, UpdateNoteRequest request)
        {
            try
            {
                var note = await _unitOfWork.NoteRepo.GetByIdAsync(noteId, responseId);
                if (note == null) throw new KeyNotFoundException($"Note with ID {noteId} not found");

                if (request.Content != null) note.Content = request.Content;
                if (request.FromIndex.HasValue) note.FromIndex = request.FromIndex.Value;
                if (request.ToIndex.HasValue) note.ToIndex = request.ToIndex.Value;

                await _unitOfWork.NoteRepo.UpdateAsync(note);
                await _unitOfWork.SaveChangesAsync();
                return note;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating note {NoteId} for response {ResponseId}", noteId, responseId);
                throw;
            }
        }

        public async Task<Note> GetNoteAsync(string noteId, string responseId)
        {
            try
            {
                return await _unitOfWork.NoteRepo.GetNoteWithResponseAsync(noteId, responseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting note {NoteId} for response {ResponseId}", noteId, responseId);
                throw;
            }
        }

        public async Task<IEnumerable<Note>> GetNotesByResponseIdAsync(string responseId)
        {
            try
            {
                return await _unitOfWork.NoteRepo.GetNotesByResponseIdAsync(responseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notes for response {ResponseId}", responseId);
                throw;
            }
        }

        public async Task<IEnumerable<Note>> GetNotesInConversationAsync(string conversationId)
        {
            try
            {
                // Get all responses in the conversation
                var responses = await _unitOfWork.AIResponseRepo.GetResponsesByConversationIdAsync(conversationId);
                var notes = new List<Note>();

                // Get notes for each response
                foreach (var response in responses)
                {
                    var responseNotes = await _unitOfWork.NoteRepo.GetNotesByResponseIdAsync(response.Id);
                    notes.AddRange(responseNotes);
                }

                return notes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notes for conversation {ConversationId}", conversationId);
                throw;
            }
        }
    }
}
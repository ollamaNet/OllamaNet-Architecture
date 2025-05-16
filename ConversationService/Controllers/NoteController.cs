using Microsoft.AspNetCore.Mvc;
using ConversationService.NoteService;
using ConversationService.NoteService.DTOs;
using Ollama_DB_layer.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConversationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NoteController : ControllerBase
    {
        private readonly INoteService _noteService;
        private readonly ILogger<NoteController> _logger;

        public NoteController(INoteService noteService, ILogger<NoteController> logger)
        {
            _noteService = noteService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<NoteResponse>> AddNote([FromBody] AddNoteRequest request)
        {
            try
            {
                var note = await _noteService.AddNoteAsync(request);
                return Ok(NoteResponse.FromEntity(note));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding note");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{responseId}/{noteId}")]
        public async Task<ActionResult> DeleteNote(string responseId, string noteId)
        {
            try
            {
                var result = await _noteService.DeleteNoteAsync(noteId, responseId);
                if (!result) return NotFound();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting note");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("soft-delete/{responseId}/{noteId}")]
        public async Task<ActionResult> SoftDeleteNote(string responseId, string noteId)
        {
            try
            {
                var result = await _noteService.SoftDeleteNoteAsync(noteId, responseId);
                if (!result) return NotFound();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting note");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{responseId}/{noteId}")]
        public async Task<ActionResult<NoteResponse>> UpdateNote(string responseId, string noteId, [FromBody] UpdateNoteRequest request)
        {
            try
            {
                var note = await _noteService.UpdateNoteAsync(noteId, responseId, request);
                return Ok(NoteResponse.FromEntity(note));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating note");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("response/{responseId}")]
        public async Task<ActionResult<IEnumerable<NoteResponse>>> GetNotesByResponseId(string responseId)
        {
            try
            {
                var notes = await _noteService.GetNotesByResponseIdAsync(responseId);
                return Ok(notes.Select(NoteResponse.FromEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notes by response ID");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{responseId}/{noteId}")]
        public async Task<ActionResult<NoteResponse>> GetNote(string responseId, string noteId)
        {
            try
            {
                var note = await _noteService.GetNoteAsync(noteId, responseId);
                if (note == null) return NotFound();
                return Ok(NoteResponse.FromEntity(note));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting note");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("conversation/{conversationId}")]
        public async Task<ActionResult<IEnumerable<NoteResponse>>> GetNotesInConversation(string conversationId)
        {
            try
            {
                var notes = await _noteService.GetNotesInConversationAsync(conversationId);
                return Ok(notes.Select(NoteResponse.FromEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notes by conversation ID");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

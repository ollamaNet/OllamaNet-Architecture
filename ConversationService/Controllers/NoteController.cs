using Microsoft.AspNetCore.Mvc;
using Ollama_DB_layer.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConversationServices.Services.NoteService;
using ConversationServices.Services.NoteService.DTOs;

namespace ConversationServices.Controllers
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteNote(string responseId, string noteId)
        {
            try
            {
                var result = await _noteService.DeleteNoteAsync(responseId, noteId);
                if (!result) return NotFound();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting note");
                return StatusCode(500, "Internal server error");
            }
        }








        [HttpDelete("soft-delete/{responseId}/{noteId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> SoftDeleteNote(string responseId, string noteId)
        {
            try
            {
                var result = await _noteService.SoftDeleteNoteAsync(responseId, noteId);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NoteResponse>> UpdateNote(string responseId, string noteId, [FromBody] UpdateNoteRequest request)
        {
            try
            {
                var note = await _noteService.UpdateNoteAsync(responseId, noteId, request);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NoteResponse>> GetNote(string responseId, string noteId)
        {
            try
            {
                var note = await _noteService.GetNoteAsync(responseId, noteId);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

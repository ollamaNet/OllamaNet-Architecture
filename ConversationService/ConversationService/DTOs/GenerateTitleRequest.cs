using System.ComponentModel.DataAnnotations;

namespace ConversationService.ConversationService.DTOs
{
    public class GenerateTitleRequest
    {
        [Required]
        public string Prompt { get; set; }

        [Required]
        public string Response { get; set; }
    }
}

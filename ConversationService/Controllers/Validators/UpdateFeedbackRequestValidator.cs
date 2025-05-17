using ConversationService.FeedbackService.DTOs;
using FluentValidation;

namespace ConversationService.Controllers.Validators
{
    public class UpdateFeedbackRequestValidator : AbstractValidator<UpdateFeedbackRequest>
    {
        public UpdateFeedbackRequestValidator()
        {
            // No specific validation rules needed for boolean Rating
            // But we could add custom rules if needed in the future
        }
    }
}
using ConversationService.FeedbackService.DTOs;
using FluentValidation;

namespace ConversationService.Controllers.Validators
{
    public class AddFeedbackRequestValidator : AbstractValidator<AddFeedbackRequest>
    {
        public AddFeedbackRequestValidator()
        {
            RuleFor(x => x.ResponseId)
                .NotEmpty().WithMessage("Response ID is required");
        }
    }
}
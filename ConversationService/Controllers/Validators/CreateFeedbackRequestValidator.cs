using ConversationService.FeedbackService.DTOs;
using FluentValidation;

namespace ConversationService.Controllers.Validators
{
    public class CreateFeedbackRequestValidator : AbstractValidator<CreateFeedbackRequest>
    {
        public CreateFeedbackRequestValidator()
        {
            RuleFor(x => x.ResponseId)
                .NotEmpty().WithMessage("Response ID is required");
        }
    }
}
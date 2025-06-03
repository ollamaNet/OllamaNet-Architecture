using ConversationServices.Services.FeedbackService.DTOs;
using FluentValidation;

namespace ConversationServices.Controllers.Validators
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
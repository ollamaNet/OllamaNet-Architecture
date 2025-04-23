using ConversationService.ChatService.DTOs;
using ConversationService.ConversationService.DTOs;
using FluentValidation;
using System;

namespace ConversationService.Controllers.Validators
{
    public class OpenConversationRequestValidator : AbstractValidator<OpenConversationRequest>
    {
        public OpenConversationRequestValidator()
        {
            RuleFor(x => x.ModelName).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        }
    }

    public class UpdateConversationRequestValidator : AbstractValidator<UpdateConversationRequest>
    {
        public UpdateConversationRequestValidator()
        {
            // At least one field must be provided
            RuleFor(x => x).Must(x => !string.IsNullOrEmpty(x.Title) || !string.IsNullOrEmpty(x.SystemMessage))
                .WithMessage("At least one field (Title or SystemMessage) must be provided");
                
            // If title is provided, validate it
            When(x => !string.IsNullOrEmpty(x.Title), () => {
                RuleFor(x => x.Title).MaximumLength(100);
            });
            
            // If system message is provided, validate it
            When(x => !string.IsNullOrEmpty(x.SystemMessage), () => {
                RuleFor(x => x.SystemMessage).MaximumLength(1000);
            });
        }
    }

    public class PromptRequestValidator : AbstractValidator<PromptRequest>
    {
        public PromptRequestValidator()
        {
            RuleFor(x => x.ConversationId).NotEmpty().Must(BeValidGuid).WithMessage("ConversationId must be a valid GUID");
            RuleFor(x => x.Model).NotEmpty();
            RuleFor(x => x.Content).NotEmpty();
        }
        
        private bool BeValidGuid(string guid) => Guid.TryParse(guid, out _);
    }
} 
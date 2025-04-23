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

    public class ChatRequestValidator : AbstractValidator<PromptRequest>
    {
        public ChatRequestValidator()
        {
            RuleFor(x => x.ConversationId).NotEmpty().Must(BeValidGuid)
                .WithMessage("ConversationId must be a valid GUID");
            RuleFor(x => x.Model).NotEmpty().WithMessage("Model name is required");
            RuleFor(x => x.Content).NotEmpty().WithMessage("Message content is required");
            
            // Validate options if they are provided
            When(x => x.Options != null, () => {
                // Temperature validation (if provided)
                When(x => x.Options.Temperature.HasValue, () => {
                    RuleFor(x => x.Options.Temperature.Value)
                        .InclusiveBetween(0.0f, 2.0f)
                        .WithMessage("Temperature must be between 0 and 2");
                });
                
                // NumPredict validation (if provided)
                When(x => x.Options.NumPredict.HasValue, () => {
                    RuleFor(x => x.Options.NumPredict.Value)
                        .GreaterThan(0)
                        .WithMessage("NumPredict must be greater than 0");
                });
                
                // RepeatLastN validation (if provided)
                When(x => x.Options.RepeatLastN.HasValue, () => {
                    RuleFor(x => x.Options.RepeatLastN.Value)
                        .GreaterThanOrEqualTo(0)
                        .WithMessage("RepeatLastN must be greater than or equal to 0");
                });
                
                // PresencePenalty validation (if provided)
                When(x => x.Options.PresencePenalty.HasValue, () => {
                    RuleFor(x => x.Options.PresencePenalty.Value)
                        .InclusiveBetween(0.0f, 1.0f)
                        .WithMessage("PresencePenalty must be between 0 and 1");
                });
            });
        }
        
        private bool BeValidGuid(string guid) => Guid.TryParse(guid, out _);
    }
} 
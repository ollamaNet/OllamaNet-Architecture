using FluentValidation;
using ConversationService.Infrastructure.Document.Options;
using ConversationService.Services.Document.DTOs.Requests;
using Microsoft.Extensions.Options;

namespace ConversationService.Controllers.Document.Validators;

/// <summary>
/// Validator for document upload requests
/// </summary>
public class DocumentRequestValidator : AbstractValidator<UploadDocumentRequest>
{
    public DocumentRequestValidator(IOptions<DocumentManagementOptions> options)
    {
        var documentOptions = options.Value;

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required");

        RuleFor(x => x.File.Length)
            .GreaterThan(0)
            .WithMessage("File cannot be empty")
            .LessThanOrEqualTo(documentOptions.MaxFileSizeBytes)
            .WithMessage($"File size must not exceed {documentOptions.MaxFileSizeBytes / 1024 / 1024} MB");

        RuleFor(x => x.File.ContentType)
            .Must(contentType => documentOptions.AllowedContentTypes.Contains(contentType))
            .WithMessage("File type is not supported");

        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("Conversation ID is required");

        When(x => x.Metadata != null, () =>
        {
            RuleForEach(x => x.Metadata)
                .Must(kvp => !string.IsNullOrWhiteSpace(kvp.Key) && !string.IsNullOrWhiteSpace(kvp.Value))
                .WithMessage("Metadata keys and values cannot be empty");
        });
    }
} 
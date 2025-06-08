using ConversationServices.Services.FolderService.DTOs;
using FluentValidation;

namespace ConversationServices.Controllers.Validators
{
    public class CreateFolderRequestValidator : AbstractValidator<CreateFolderRequest>
    {
        public CreateFolderRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Folder name is required")
                .MaximumLength(100).WithMessage("Folder name cannot exceed 100 characters")
                .Matches("^[a-zA-Z0-9\\s-_]+$").WithMessage("Folder name can only contain letters, numbers, spaces, hyphens, and underscores");

            RuleFor(x => x.RootFolderId)
                .NotEmpty().WithMessage("Root folder ID is required");
        }
    }
} 
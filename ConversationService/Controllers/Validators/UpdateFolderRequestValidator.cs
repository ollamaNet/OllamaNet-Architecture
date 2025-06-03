using ConversationServices.Services.FolderService.DTOs;
using FluentValidation;

namespace ConversationServices.Controllers.Validators
{
    public class UpdateFolderRequestValidator : AbstractValidator<UpdateFolderRequest>
    {
        public UpdateFolderRequestValidator()
        {
            RuleFor(x => x.FolderId)
                .NotEmpty().WithMessage("Folder ID is required");

            RuleFor(x => x.NewName)
                .NotEmpty().WithMessage("New folder name is required")
                .MaximumLength(100).WithMessage("Folder name cannot exceed 100 characters")
                .Matches("^[a-zA-Z0-9\\s-_]+$").WithMessage("Folder name can only contain letters, numbers, spaces, hyphens, and underscores");
        }
    }
}
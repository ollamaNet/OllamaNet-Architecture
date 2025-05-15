using AdminService.Services.UserOperations.DTOs;
using FluentValidation;

namespace AdminService.Controllers.Validators
{
    public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
    {
        public CreateRoleRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required")
                .MinimumLength(3).WithMessage("Role name must be at least 3 characters")
                .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters")
                .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Role name can only contain letters, numbers, underscores, and hyphens");
                
            RuleFor(x => x.Description)
                .MaximumLength(200).WithMessage("Description cannot exceed 200 characters")
                .When(x => x.Description != null);
        }
    }
}
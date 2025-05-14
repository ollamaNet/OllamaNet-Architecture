using AdminService.Services.UserOperations.DTOs;
using FluentValidation;
using System;

namespace AdminService.Controllers.Validators
{
    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("A valid email address is required");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters")
                .MaximumLength(50).WithMessage("Username cannot exceed 50 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required");
        }
    }

    public class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
    {
        public UpdateUserProfileRequestValidator()
        {
            // At least one field must be provided
            RuleFor(x => x).Must(x => 
                !string.IsNullOrEmpty(x.FirstName) || 
                !string.IsNullOrEmpty(x.LastName) || 
                !string.IsNullOrEmpty(x.Email) || 
                !string.IsNullOrEmpty(x.UserName))
                .WithMessage("At least one field must be provided");

            // Email validation
            When(x => !string.IsNullOrEmpty(x.Email), () => {
                RuleFor(x => x.Email)
                    .EmailAddress().WithMessage("A valid email address is required");
            });

            // Username validation
            When(x => !string.IsNullOrEmpty(x.UserName), () => {
                RuleFor(x => x.UserName)
                    .MinimumLength(3).WithMessage("Username must be at least 3 characters")
                    .MaximumLength(50).WithMessage("Username cannot exceed 50 characters");
            });

            // First name validation
            When(x => !string.IsNullOrEmpty(x.FirstName), () => {
                RuleFor(x => x.FirstName)
                    .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");
            });

            // Last name validation
            When(x => !string.IsNullOrEmpty(x.LastName), () => {
                RuleFor(x => x.LastName)
                    .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");
            });
        }
    }

    public class ChangeUserRoleRequestValidator : AbstractValidator<ChangeUserRoleRequest>
    {
        public ChangeUserRoleRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.NewRole)
                .NotEmpty().WithMessage("New role is required");
        }
    }

    public class ToggleUserStatusRequestValidator : AbstractValidator<ToggleUserStatusRequest>
    {
        public ToggleUserStatusRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");
        }
    }

    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number");
        }
    }

    public class LockUserRequestValidator : AbstractValidator<LockUserRequest>
    {
        public LockUserRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.LockoutMinutes)
                .GreaterThan(0).WithMessage("Lockout minutes must be greater than 0")
                .LessThanOrEqualTo(43200).WithMessage("Lockout cannot exceed 30 days (43200 minutes)");
        }
    }
} 
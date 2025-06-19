using AdminService.Services.AIModelOperations.DTOs;
using FluentValidation;

namespace AdminService.Controllers.Validators
{
    public class CreateModelRequestValidator : AbstractValidator<CreateModelRequest>
    {
        public CreateModelRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Model name is required")
                .MaximumLength(100).WithMessage("Model name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

            RuleFor(x => x.Version)
                .MaximumLength(50).WithMessage("Version cannot exceed 50 characters");

            RuleFor(x => x.Size)
                .MaximumLength(50).WithMessage("Size cannot exceed 50 characters");

            RuleFor(x => x.License)
                .MaximumLength(2000).WithMessage("License cannot exceed 2000 characters");

            RuleFor(x => x.Template)
                .MaximumLength(2000).When(x => x.Template != null)
                .WithMessage("Template cannot exceed 2000 characters");

            RuleFor(x => x.ModelFile)
                .MaximumLength(2000).When(x => x.ModelFile != null)
                .WithMessage("Model file path cannot exceed 2000 characters");

            RuleFor(x => x.ReferenceLink)
                .MaximumLength(500).When(x => x.ReferenceLink != null)
                .WithMessage("Reference link cannot exceed 500 characters")
                .Must(BeValidUrl).When(x => !string.IsNullOrEmpty(x.ReferenceLink))
                .WithMessage("Reference link must be a valid URL");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500).When(x => x.ImageUrl != null)
                .WithMessage("Image URL cannot exceed 500 characters")
                .Must(BeValidUrl).When(x => !string.IsNullOrEmpty(x.ImageUrl))
                .WithMessage("Image URL must be a valid URL");

            RuleFor(x => x.Digest)
                .MaximumLength(200).When(x => x.Digest != null)
                .WithMessage("Digest cannot exceed 200 characters");

            RuleFor(x => x.Format)
                .MaximumLength(50).When(x => x.Format != null)
                .WithMessage("Format cannot exceed 50 characters");

            RuleFor(x => x.ParameterSize)
                .MaximumLength(50).When(x => x.ParameterSize != null)
                .WithMessage("Parameter size cannot exceed 50 characters");

            RuleFor(x => x.QuantizationLevel)
                .MaximumLength(50).When(x => x.QuantizationLevel != null)
                .WithMessage("Quantization level cannot exceed 50 characters");

            RuleFor(x => x.ParentModel)
                .MaximumLength(100).When(x => x.ParentModel != null)
                .WithMessage("Parent model cannot exceed 100 characters");

            RuleFor(x => x.Family)
                .MaximumLength(100).When(x => x.Family != null)
                .WithMessage("Family cannot exceed 100 characters");

            RuleFor(x => x.Architecture)
                .MaximumLength(100).When(x => x.Architecture != null)
                .WithMessage("Architecture cannot exceed 100 characters");

            RuleFor(x => x.ParameterCount)
                .GreaterThan(0).When(x => x.ParameterCount.HasValue)
                .WithMessage("Parameter count must be greater than 0");

            RuleFor(x => x.SizeLabel)
                .MaximumLength(50).When(x => x.SizeLabel != null)
                .WithMessage("Size label cannot exceed 50 characters");

            RuleFor(x => x.ModelType)
                .MaximumLength(50).When(x => x.ModelType != null)
                .WithMessage("Model type cannot exceed 50 characters");

            When(x => x.Families != null && x.Families.Count > 0, () => {
                RuleForEach(x => x.Families)
                    .NotEmpty().WithMessage("Family name cannot be empty")
                    .MaximumLength(100).WithMessage("Family name cannot exceed 100 characters");
            });

            When(x => x.Languages != null && x.Languages.Count > 0, () => {
                RuleForEach(x => x.Languages)
                    .NotEmpty().WithMessage("Language name cannot be empty")
                    .MaximumLength(50).WithMessage("Language name cannot exceed 50 characters");
            });

            When(x => x.Tags != null && x.Tags.Count > 0, () => {
                RuleForEach(x => x.Tags).SetValidator(new AddTagRequestValidator());
            });
                
            // Additional validation for FromOllama scenarios
            When(x => x.FromOllama, () => {
                RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Model name is required for Ollama models");
            });
        }

        private bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }

    public class AddTagRequestValidator : AbstractValidator<AddTagRequest>
    {
        public AddTagRequestValidator()
        {
            RuleFor(x => x.TagId)
                .NotEmpty().WithMessage("Tag ID is required");
        }
    }

    public class UpdateModelRequestValidator : AbstractValidator<UpdateModelRequest>
    {
        public UpdateModelRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Model name is required");

            // At least one field must be provided
            RuleFor(x => x).Must(x => 
                x.Description != null || 
                x.Version != null ||
                x.Size != null ||
                x.ReleasedAt != null || 
                x.License != null ||
                x.Template != null ||
                x.ModelFile != null ||
                x.ReferenceLink != null ||
                x.ImageUrl != null ||
                x.Digest != null ||
                x.Format != null ||
                x.ParameterSize != null ||
                x.QuantizationLevel != null ||
                x.ParentModel != null ||
                x.Family != null ||
                x.Families != null ||
                x.Languages != null ||
                x.Architecture != null ||
                x.FileType != null ||
                x.ParameterCount != null ||
                x.QuantizationVersion != null ||
                x.SizeLabel != null ||
                x.ModelType != null)
                .WithMessage("At least one field must be provided for update");

            When(x => x.Description != null, () => {
                RuleFor(x => x.Description)
                    .MaximumLength(1000).WithMessage("Description cannot exceed 500 characters");
            });

            When(x => x.Version != null, () => {
                RuleFor(x => x.Version)
                    .MaximumLength(50).WithMessage("Version cannot exceed 50 characters");
            });

            When(x => x.Size != null, () => {
                RuleFor(x => x.Size)
                    .MaximumLength(50).WithMessage("Size cannot exceed 50 characters");
            });

            When(x => x.License != null, () => {
                RuleFor(x => x.License)
                    .MaximumLength(2000).WithMessage("License cannot exceed 100 characters");
            });

            When(x => x.Template != null, () => {
                RuleFor(x => x.Template)
                    .MaximumLength(2000).WithMessage("Template cannot exceed 1000 characters");
            });

            When(x => x.ModelFile != null, () => {
                RuleFor(x => x.ModelFile)
                    .MaximumLength(2000).WithMessage("Model file path cannot exceed 500 characters");
            });

            When(x => x.ReferenceLink != null, () => {
                RuleFor(x => x.ReferenceLink)
                    .MaximumLength(500).WithMessage("Reference link cannot exceed 500 characters")
                    .Must(BeValidUrl).When(x => !string.IsNullOrEmpty(x.ReferenceLink))
                    .WithMessage("Reference link must be a valid URL");
            });

            When(x => x.ImageUrl != null, () => {
                RuleFor(x => x.ImageUrl)
                    .MaximumLength(500).WithMessage("Image URL cannot exceed 500 characters")
                    .Must(BeValidUrl).When(x => !string.IsNullOrEmpty(x.ImageUrl))
                    .WithMessage("Image URL must be a valid URL");
            });

            When(x => x.Digest != null, () => {
                RuleFor(x => x.Digest)
                    .MaximumLength(100).WithMessage("Digest cannot exceed 100 characters");
            });

            When(x => x.Format != null, () => {
                RuleFor(x => x.Format)
                    .MaximumLength(50).WithMessage("Format cannot exceed 50 characters");
            });

            When(x => x.ParameterSize != null, () => {
                RuleFor(x => x.ParameterSize)
                    .MaximumLength(50).WithMessage("Parameter size cannot exceed 50 characters");
            });

            When(x => x.QuantizationLevel != null, () => {
                RuleFor(x => x.QuantizationLevel)
                    .MaximumLength(50).WithMessage("Quantization level cannot exceed 50 characters");
            });

            When(x => x.ParentModel != null, () => {
                RuleFor(x => x.ParentModel)
                    .MaximumLength(100).WithMessage("Parent model cannot exceed 100 characters");
            });

            When(x => x.Family != null, () => {
                RuleFor(x => x.Family)
                    .MaximumLength(100).WithMessage("Family cannot exceed 100 characters");
            });

            When(x => x.Architecture != null, () => {
                RuleFor(x => x.Architecture)
                    .MaximumLength(100).WithMessage("Architecture cannot exceed 100 characters");
            });

            When(x => x.ParameterCount != null, () => {
                RuleFor(x => x.ParameterCount)
                    .GreaterThan(0).WithMessage("Parameter count must be greater than 0");
            });

            When(x => x.SizeLabel != null, () => {
                RuleFor(x => x.SizeLabel)
                    .MaximumLength(50).WithMessage("Size label cannot exceed 50 characters");
            });

            When(x => x.ModelType != null, () => {
                RuleFor(x => x.ModelType)
                    .MaximumLength(50).WithMessage("Model type cannot exceed 50 characters");
            });

            When(x => x.Families != null && x.Families.Count > 0, () => {
                RuleForEach(x => x.Families)
                    .NotEmpty().WithMessage("Family name cannot be empty")
                    .MaximumLength(100).WithMessage("Family name cannot exceed 100 characters");
            });

            When(x => x.Languages != null && x.Languages.Count > 0, () => {
                RuleForEach(x => x.Languages)
                    .NotEmpty().WithMessage("Language name cannot be empty")
                    .MaximumLength(50).WithMessage("Language name cannot exceed 50 characters");
            });
        }

        private bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }

    public class ModelTagOperationRequestValidator : AbstractValidator<ModelTagOperationRequest>
    {
        public ModelTagOperationRequestValidator()
        {
            RuleFor(x => x.ModelId)
                .NotEmpty().WithMessage("Model ID is required");

            RuleFor(x => x.TagIds)
                .NotEmpty().WithMessage("At least one tag ID must be provided")
                .Must(x => x != null && x.Count > 0).WithMessage("Tag IDs list cannot be empty");

            RuleForEach(x => x.TagIds)
                .NotEmpty().WithMessage("Tag ID cannot be empty");
        }
    }

    public class SearchModelRequestValidator : AbstractValidator<SearchModelRequest>
    {
        public SearchModelRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");

            When(x => x.TagIds != null && x.TagIds.Count > 0, () => {
                RuleForEach(x => x.TagIds)
                    .NotEmpty().WithMessage("Tag ID cannot be empty");
            });

            When(x => !string.IsNullOrEmpty(x.SearchTerm), () => {
                RuleFor(x => x.SearchTerm)
                    .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters");
            });

            When(x => !string.IsNullOrEmpty(x.OwnerId), () => {
                RuleFor(x => x.OwnerId)
                    .MaximumLength(100).WithMessage("Owner ID cannot exceed 100 characters");
            });
        }
    }
} 
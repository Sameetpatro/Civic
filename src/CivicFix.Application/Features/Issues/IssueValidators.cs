using FluentValidation;

namespace CivicFix.Application.Features.Issues;

public class CreateIssueRequestValidator : AbstractValidator<CreateIssueRequestDto>
{
    public CreateIssueRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Issue title is required.")
            .MaximumLength(250).WithMessage("Title cannot exceed 250 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Issue description is required.")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category must be selected.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90.0, 90.0).WithMessage("Valid latitude required.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180.0, 180.0).WithMessage("Valid longitude required.");

        RuleFor(x => x.WardSector)
            .NotEmpty().WithMessage("Ward or Sector is required (e.g. 'Sector 14').");
    }
}

public class ResolveIssueRequestValidator : AbstractValidator<ResolveIssueRequestDto>
{
    public ResolveIssueRequestValidator()
    {
        RuleFor(x => x.ResolutionNotes)
            .NotEmpty().WithMessage("Resolution notes are required to mark an issue resolved.")
            .MinimumLength(10).WithMessage("Resolution notes must be at least 10 characters.");
    }
}

public class VerifyIssueRequestValidator : AbstractValidator<VerifyIssueRequestDto>
{
    public VerifyIssueRequestValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).When(x => x.Rating.HasValue)
            .WithMessage("Rating must be between 1 and 5 stars.");
    }
}

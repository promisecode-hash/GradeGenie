using FluentValidation;
using GradeGenie.Application.DTOs;
using GradeGenie.Domain.Services;

namespace GradeGenie.Application.Validators;

public sealed class ConvertScaleValidator : AbstractValidator<ConvertScaleRequest>
{
    public ConvertScaleValidator()
    {
        RuleFor(x => x.Value)
            .Must((req, val) => val >= 0 && val <= req.SourceScale.Max())
            .WithMessage((req, val) => $"Value {val} is out of range for source scale {req.SourceScale.Max()}");
    }
}

public sealed class ScaleConversionValidator : AbstractValidator<ScaleConversionRequest>
{
    public ScaleConversionValidator()
    {
        RuleFor(x => x.Value)
            .Must((req, val) => val >= 0 && val <= req.SourceScale.Max())
            .WithMessage((req, val) => $"Value {val} is out of range for source scale {req.SourceScale.Max()}");
    }
}

using FluentValidation;
using GradeGenie.Application.DTOs;

namespace GradeGenie.Application.Validators;

public sealed class CourseValidator : AbstractValidator<CreateCourseRequest>
{
    public CourseValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Course code is required.");
        RuleFor(x => x.CreditUnits).GreaterThan(0).WithMessage("Credit units must be greater than 0.");
    }
}

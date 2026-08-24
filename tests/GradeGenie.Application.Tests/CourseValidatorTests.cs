using GradeGenie.Domain.Entities;
using GradeGenie.Application.DTOs;
using GradeGenie.Application.Validators;
using Xunit;

namespace GradeGenie.Application.Tests;

public sealed class CourseValidatorTests
{
    [Fact]
    public void Validator_rejects_zero_credit_units()
    {
        var validator = new CourseValidator();
        var req = new CreateCourseRequest("CSC101", "Intro", 0, Grade.A);
        var result = validator.Validate(req);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_accepts_valid_course()
    {
        var validator = new CourseValidator();
        var req = new CreateCourseRequest("CSC101", "Intro", 3, Grade.B);
        var result = validator.Validate(req);
        Assert.True(result.IsValid);
    }
}

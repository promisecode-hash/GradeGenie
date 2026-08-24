using GradeGenie.Application.DTOs;
using GradeGenie.Application.Validators;
using GradeGenie.Domain.Services;
using Xunit;

namespace GradeGenie.Application.Tests;

public sealed class ConvertScaleValidatorTests
{
    [Fact]
    public void Validator_rejects_out_of_range_value()
    {
        var validator = new ConvertScaleValidator();
        var req = new ConvertScaleRequest(5.1m, GradingScale.FivePoint, GradingScale.FourPoint);
        var result = validator.Validate(req);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_accepts_in_range_value()
    {
        var validator = new ConvertScaleValidator();
        var req = new ConvertScaleRequest(4.2m, GradingScale.FivePoint, GradingScale.FourPoint);
        var result = validator.Validate(req);
        Assert.True(result.IsValid);
    }
}

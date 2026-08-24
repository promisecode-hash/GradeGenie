using GradeGenie.Domain.Services;
using Xunit;

namespace GradeGenie.Domain.Tests;

public sealed class ScaleConverterTests
{
    [Theory]
    [InlineData(5.0, GradingScale.FivePoint, GradingScale.FourPoint, 4.00)]
    [InlineData(4.2, GradingScale.FivePoint, GradingScale.FourPoint, 3.36)]
    public void Converts_between_scales(decimal value, GradingScale source, GradingScale target, decimal expected)
    {
        var converted = ScaleConverter.Convert(value, source, target);
        Assert.Equal(expected, converted);
    }

    [Fact]
    public void Throws_on_out_of_range_value()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ScaleConverter.Convert(5.1m, GradingScale.FivePoint, GradingScale.FourPoint));
    }
}

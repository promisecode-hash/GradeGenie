namespace GradeGenie.Domain.Services;

public enum GradingScale
{
    FourPoint,
    FivePoint,
    TenPoint,
    TwelvePoint,
    Percentage
}

public static class GradingScaleExtensions
{
    public static decimal Max(this GradingScale scale) => scale switch
    {
        GradingScale.FourPoint => 4m,
        GradingScale.FivePoint => 5m,
        GradingScale.TenPoint => 10m,
        GradingScale.TwelvePoint => 12m,
        GradingScale.Percentage => 100m,
        _ => throw new ArgumentOutOfRangeException(nameof(scale))
    };
}

public static class ScaleConverter
{
    public static decimal Convert(decimal value, GradingScale source, GradingScale target)
    {
        var sourceMax = source.Max();
        var targetMax = target.Max();
        if (value < 0m || value > sourceMax) throw new ArgumentOutOfRangeException(nameof(value), $"Value {value} is out of range for source scale {sourceMax}");
        if (source == target) return decimal.Round(value, 2);

        var converted = (value / sourceMax) * targetMax;
        return decimal.Round(converted, 2);
    }
}

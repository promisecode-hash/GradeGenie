namespace GradeGenie.Domain.Entities;

public static class GradePointScale
{
    public static decimal GetPoint(Grade grade) => grade switch
    {
        Grade.A => 5m,
        Grade.B => 4m,
        Grade.C => 3m,
        Grade.D => 2m,
        Grade.E => 1m,
        Grade.F => 0m,
        _ => throw new ArgumentOutOfRangeException(nameof(grade))
    };
}

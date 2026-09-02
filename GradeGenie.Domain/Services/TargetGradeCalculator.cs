namespace GradeGenie.Domain.Services;

public static class TargetGradeCalculator
{
    public static decimal CalculateRequiredGradePoint(decimal currentCgpa, decimal targetCgpa, decimal completedCreditUnits, decimal remainingCreditUnits)
    {
        if (completedCreditUnits < 0m) throw new ArgumentOutOfRangeException(nameof(completedCreditUnits));
        if (remainingCreditUnits < 0m) throw new ArgumentOutOfRangeException(nameof(remainingCreditUnits));
        if (targetCgpa < 0m) throw new ArgumentOutOfRangeException(nameof(targetCgpa));
        if (currentCgpa < 0m) throw new ArgumentOutOfRangeException(nameof(currentCgpa));
        if (remainingCreditUnits == 0m) return 0m;

        var totalPointsNeeded = targetCgpa * (completedCreditUnits + remainingCreditUnits);
        var currentPoints = currentCgpa * completedCreditUnits;
        var requiredAverage = (totalPointsNeeded - currentPoints) / remainingCreditUnits;

        return decimal.Round(Math.Max(0m, requiredAverage), 2);
    }

    public static string GetLetterGrade(decimal gradePoint)
    {
        return gradePoint switch
        {
            >= 4.5m => "A",
            >= 3.5m => "B",
            >= 2.5m => "C",
            >= 1.5m => "D",
            _ => "F"
        };
    }
}

namespace GradeGenie.Domain.Services;

public sealed record AcademicPlan(decimal RequiredGradePoint, string RecommendedPriority, string Summary);

public static class AcademicPlanner
{
    public static AcademicPlan CreatePlan(decimal currentCgpa, decimal targetCgpa, decimal completedCreditUnits, decimal remainingCreditUnits)
    {
        var requiredGradePoint = TargetGradeCalculator.CalculateRequiredGradePoint(currentCgpa, targetCgpa, completedCreditUnits, remainingCreditUnits);
        var priority = requiredGradePoint switch
        {
            > 4.5m => "A-range focus",
            > 3.5m => "B-range focus",
            > 2.5m => "C-range focus",
            > 1.5m => "D-range focus",
            _ => "Pass and protect your GPA"
        };

        return new AcademicPlan(
            requiredGradePoint,
            priority,
            $"You need to average about {requiredGradePoint:0.00} grade points in the remaining {remainingCreditUnits} credit units to reach {targetCgpa:0.00} CGPA.");
    }
}

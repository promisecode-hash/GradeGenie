using GradeGenie.Domain.Entities;

namespace GradeGenie.Domain.Services;

public static class CgpaCalculator
{
    public static CgpaResult Calculate(IEnumerable<Course>? courses)
    {
        var list = (courses ?? Array.Empty<Course>()).ToArray();
        var breakdown = list.Select(c => new CourseContribution(c.Code, c.GradePoint, c.CreditUnits, decimal.Round(c.QualityPoints, 2))).ToArray();
        var totalUnits = list.Sum(c => c.CreditUnits);
        if (totalUnits == 0) return new CgpaResult(0m, breakdown);
        var totalWeighted = list.Sum(c => c.QualityPoints);
        var cgpa = decimal.Round(totalWeighted / totalUnits, 2);
        return new CgpaResult(cgpa, breakdown);
    }
}

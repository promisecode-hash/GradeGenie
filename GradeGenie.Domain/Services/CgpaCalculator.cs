using GradeGenie.Domain.Entities;

namespace GradeGenie.Domain.Services;

public static class CgpaCalculator
{
    public static CgpaResult Calculate(IEnumerable<Course>? courses, EducationInstitutionType? institutionType = null)
    {
        var list = (courses ?? Array.Empty<Course>()).ToArray();

        CourseContribution[] breakdown;
        if (institutionType is null)
        {
            breakdown = list.Select(c => new CourseContribution(c.Code, c.GradePoint, c.CreditUnits, decimal.Round(c.QualityPoints, 2))).ToArray();
        }
        else
        {
            breakdown = list.Select(c => new CourseContribution(c.Code, c.GradePointFor(institutionType.Value), c.CreditUnits, decimal.Round(c.QualityPointsFor(institutionType.Value), 2))).ToArray();
        }

        var totalUnits = list.Sum(c => c.CreditUnits);
        if (totalUnits == 0) return new CgpaResult(0m, breakdown);

        var totalWeighted = institutionType is null ? list.Sum(c => c.QualityPoints) : list.Sum(c => c.QualityPointsFor(institutionType.Value));
        var cgpa = decimal.Round(totalWeighted / totalUnits, 2);
        return new CgpaResult(cgpa, breakdown);
    }
}

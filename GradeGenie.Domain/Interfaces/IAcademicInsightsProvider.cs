using GradeGenie.Domain.Entities;

namespace GradeGenie.Domain.Interfaces;

public interface IAcademicInsightsProvider
{
    Task<string> GenerateSemesterInsightAsync(Student student, Semester semester, CancellationToken cancellationToken = default);
}

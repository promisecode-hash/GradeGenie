using GradeGenie.Application.DTOs;

namespace GradeGenie.Application.Services;

public interface IStudentAcademicService
{
    Task<StudentCgpaDto?> CalculateCgpaAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<SemesterInsightDto?> GenerateSemesterInsightAsync(Guid studentId, Guid semesterId, CancellationToken cancellationToken = default);
}

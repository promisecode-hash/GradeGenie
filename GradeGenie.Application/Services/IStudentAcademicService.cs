using GradeGenie.Application.DTOs;

namespace GradeGenie.Application.Services;

public interface IStudentAcademicService
{
    Task<StudentDto> CreateStudentAsync(string userId, CreateStudentRequest request, CancellationToken cancellationToken = default);
    Task<SemesterDto?> AddSemesterAsync(string userId, Guid studentId, CreateSemesterRequest request, CancellationToken cancellationToken = default);
    Task<CourseDto?> AddCourseAsync(string userId, Guid studentId, Guid semesterId, CreateCourseRequest request, CancellationToken cancellationToken = default);
    Task<StudentCgpaDto?> CalculateCgpaAsync(string userId, Guid studentId, CancellationToken cancellationToken = default);
    Task<SemesterInsightDto?> GenerateSemesterInsightAsync(string userId, Guid studentId, Guid semesterId, CancellationToken cancellationToken = default);
}

using GradeGenie.Domain.Entities;

namespace GradeGenie.Domain.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetWithSemestersForUserAsync(Guid studentId, string userId, CancellationToken cancellationToken = default);
    Task AddAsync(Student student, CancellationToken cancellationToken = default);
    Task AddSemesterAsync(Semester semester, CancellationToken cancellationToken = default);
    Task AddCourseAsync(Course course, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

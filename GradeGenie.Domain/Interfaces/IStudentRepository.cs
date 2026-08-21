using GradeGenie.Domain.Entities;

namespace GradeGenie.Domain.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetWithSemestersAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task AddAsync(Student student, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

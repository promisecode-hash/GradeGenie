using GradeGenie.Domain.Entities;
using GradeGenie.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GradeGenie.Infrastructure.Persistence;

public sealed class StudentRepository(GradeGenieDbContext dbContext) : IStudentRepository
{
    public Task<Student?> GetWithSemestersAsync(Guid studentId, CancellationToken cancellationToken = default) =>
        dbContext.Students.Include(student => student.Semesters).ThenInclude(semester => semester.Courses)
            .SingleOrDefaultAsync(student => student.Id == studentId, cancellationToken);
    public Task AddAsync(Student student, CancellationToken cancellationToken = default) => dbContext.Students.AddAsync(student, cancellationToken).AsTask();
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);
}

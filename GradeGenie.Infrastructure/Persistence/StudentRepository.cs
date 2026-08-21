using GradeGenie.Domain.Entities;
using GradeGenie.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GradeGenie.Infrastructure.Persistence;

public sealed class StudentRepository(GradeGenieDbContext dbContext) : IStudentRepository
{
    public Task<Student?> GetWithSemestersForUserAsync(Guid studentId, string userId, CancellationToken cancellationToken = default) =>
        dbContext.Students.Include(student => student.Semesters).ThenInclude(semester => semester.Courses)
            .SingleOrDefaultAsync(student => student.Id == studentId && student.UserId == userId, cancellationToken);
    public Task AddAsync(Student student, CancellationToken cancellationToken = default) => dbContext.Students.AddAsync(student, cancellationToken).AsTask();
    public Task AddSemesterAsync(Semester semester, CancellationToken cancellationToken = default) => dbContext.Semesters.AddAsync(semester, cancellationToken).AsTask();
    public Task AddCourseAsync(Course course, CancellationToken cancellationToken = default) => dbContext.Courses.AddAsync(course, cancellationToken).AsTask();
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);
}

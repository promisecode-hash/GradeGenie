using GradeGenie.Application.DTOs;
using GradeGenie.Domain.Entities;
using GradeGenie.Domain.Interfaces;

namespace GradeGenie.Application.Services;

public sealed class StudentAcademicService(IStudentRepository students, IAcademicInsightsProvider insights) : IStudentAcademicService
{
    public async Task<StudentCgpaDto?> CalculateCgpaAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var student = await students.GetWithSemestersAsync(studentId, cancellationToken);
        return student is null ? null : Map(student);
    }

    public async Task<SemesterInsightDto?> GenerateSemesterInsightAsync(Guid studentId, Guid semesterId, CancellationToken cancellationToken = default)
    {
        var student = await students.GetWithSemestersAsync(studentId, cancellationToken);
        var semester = student?.Semesters.SingleOrDefault(item => item.Id == semesterId);
        if (student is null || semester is null) return null;
        var insight = await insights.GenerateSemesterInsightAsync(student, semester, cancellationToken);
        return new SemesterInsightDto(semester.Id, semester.Gpa, insight);
    }

    private static StudentCgpaDto Map(Student student) => new(student.Id, student.FullName, student.Cgpa,
        student.Semesters.Select(semester => new SemesterDto(semester.Id, semester.Name, semester.AcademicYear, semester.Gpa, semester.TotalCreditUnits,
            semester.Courses.Select(course => new CourseDto(course.Id, course.Code, course.Title, course.CreditUnits, course.Grade, course.GradePoint)).ToArray())).ToArray());
}

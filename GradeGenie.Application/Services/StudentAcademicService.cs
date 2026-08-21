using GradeGenie.Application.DTOs;
using GradeGenie.Domain.Entities;
using GradeGenie.Domain.Interfaces;

namespace GradeGenie.Application.Services;

public sealed class StudentAcademicService(IStudentRepository students, IAcademicInsightsProvider insights) : IStudentAcademicService
{
    public async Task<StudentDto> CreateStudentAsync(string userId, CreateStudentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var student = new Student(userId, request.FullName);
        await students.AddAsync(student, cancellationToken);
        await students.SaveChangesAsync(cancellationToken);
        return new StudentDto(student.Id, student.UserId, student.FullName);
    }

    public async Task<SemesterDto?> AddSemesterAsync(string userId, Guid studentId, CreateSemesterRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var student = await students.GetWithSemestersForUserAsync(studentId, userId, cancellationToken);
        if (student is null) return null;
        var semester = new Semester(request.Name, request.AcademicYear);
        student.AddSemester(semester);
        await students.AddSemesterAsync(semester, cancellationToken);
        await students.SaveChangesAsync(cancellationToken);
        return Map(semester);
    }

    public async Task<CourseDto?> AddCourseAsync(string userId, Guid studentId, Guid semesterId, CreateCourseRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var student = await students.GetWithSemestersForUserAsync(studentId, userId, cancellationToken);
        var semester = student?.Semesters.SingleOrDefault(item => item.Id == semesterId);
        if (semester is null) return null;
        var course = new Course(request.Code, request.Title, request.CreditUnits, request.Grade);
        semester.AddCourse(course);
        await students.AddCourseAsync(course, cancellationToken);
        await students.SaveChangesAsync(cancellationToken);
        return Map(course);
    }

    public async Task<StudentCgpaDto?> CalculateCgpaAsync(string userId, Guid studentId, CancellationToken cancellationToken = default)
    {
        var student = await students.GetWithSemestersForUserAsync(studentId, userId, cancellationToken);
        return student is null ? null : Map(student);
    }

    public async Task<SemesterInsightDto?> GenerateSemesterInsightAsync(string userId, Guid studentId, Guid semesterId, CancellationToken cancellationToken = default)
    {
        var student = await students.GetWithSemestersForUserAsync(studentId, userId, cancellationToken);
        var semester = student?.Semesters.SingleOrDefault(item => item.Id == semesterId);
        if (student is null || semester is null) return null;
        var insight = await insights.GenerateSemesterInsightAsync(student, semester, cancellationToken);
        return new SemesterInsightDto(semester.Id, semester.Gpa, insight);
    }

    private static StudentCgpaDto Map(Student student) => new(student.Id, student.FullName, student.Cgpa, student.Semesters.Select(Map).ToArray());
    private static SemesterDto Map(Semester semester) => new(semester.Id, semester.Name, semester.AcademicYear, semester.Gpa, semester.TotalCreditUnits, semester.Courses.Select(Map).ToArray());
    private static CourseDto Map(Course course) => new(course.Id, course.Code, course.Title, course.CreditUnits, course.Grade, course.GradePoint);
}

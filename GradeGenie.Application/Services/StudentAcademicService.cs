using GradeGenie.Application.DTOs;
using GradeGenie.Domain.Entities;
using GradeGenie.Domain.Interfaces;
using GradeGenie.Domain.Services;

namespace GradeGenie.Application.Services;

public sealed class StudentAcademicService(IStudentRepository students, IAcademicInsightsProvider insights) : IStudentAcademicService
{
    public async Task<StudentDto> CreateStudentAsync(string userId, CreateStudentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var student = new Student(userId, request.FullName, request.InstitutionType);
        await students.AddAsync(student, cancellationToken);
        await students.SaveChangesAsync(cancellationToken);
        return new StudentDto(student.Id, student.UserId, student.FullName, student.InstitutionType);
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

    public Task<TargetGradeResponse> CalculateTargetGradeAsync(TargetGradeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var requiredGradePoint = TargetGradeCalculator.CalculateRequiredGradePoint(
            request.CurrentCgpa,
            request.TargetCgpa,
            request.CompletedCreditUnits,
            request.RemainingCreditUnits);

        return Task.FromResult(new TargetGradeResponse(requiredGradePoint, TargetGradeCalculator.GetLetterGrade(requiredGradePoint)));
    }

    public Task<AcademicPlanResponse> CreateAcademicPlanAsync(AcademicPlanRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var plan = AcademicPlanner.CreatePlan(
            request.CurrentCgpa,
            request.TargetCgpa,
            request.CompletedCreditUnits,
            request.RemainingCreditUnits);

        return Task.FromResult(new AcademicPlanResponse(plan.RequiredGradePoint, plan.RecommendedPriority, plan.Summary));
    }

    public async Task<SemesterInsightDto?> GenerateSemesterInsightAsync(string userId, Guid studentId, Guid semesterId, CancellationToken cancellationToken = default)
    {
        var student = await students.GetWithSemestersForUserAsync(studentId, userId, cancellationToken);
        var semester = student?.Semesters.SingleOrDefault(item => item.Id == semesterId);
        if (student is null || semester is null) return null;
        var insight = await insights.GenerateSemesterInsightAsync(student, semester, cancellationToken);
        return new SemesterInsightDto(semester.Id, semester.GpaFor(student.InstitutionType), insight);
    }

    private static StudentCgpaDto Map(Student student) => new(student.Id, student.FullName, student.Cgpa, student.InstitutionType, student.Semesters.Select(s => Map(s, student.InstitutionType)).ToArray());
    private static SemesterDto Map(Semester semester, EducationInstitutionType institutionType) => new(semester.Id, semester.Name, semester.AcademicYear, semester.GpaFor(institutionType), semester.TotalCreditUnits, semester.Courses.Select(Map).ToArray());
    private static CourseDto Map(Course course) => new(course.Id, course.Code, course.Title, course.CreditUnits, course.Grade, course.GradePoint);
}

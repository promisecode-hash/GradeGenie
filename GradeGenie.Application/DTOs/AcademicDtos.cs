using GradeGenie.Domain.Entities;

namespace GradeGenie.Application.DTOs;

public sealed record CourseDto(Guid Id, string Code, string Title, decimal CreditUnits, Grade Grade, decimal GradePoint);
public sealed record SemesterDto(Guid Id, string Name, int AcademicYear, decimal Gpa, decimal TotalCreditUnits, IReadOnlyCollection<CourseDto> Courses);
public sealed record StudentCgpaDto(Guid StudentId, string FullName, decimal Cgpa, IReadOnlyCollection<SemesterDto> Semesters);
public sealed record SemesterInsightDto(Guid SemesterId, decimal Gpa, string Insight);
public sealed record StudentDto(Guid Id, string UserId, string FullName);
public sealed record CreateStudentRequest(string FullName);
public sealed record CreateSemesterRequest(string Name, int AcademicYear);
public sealed record CreateCourseRequest(string Code, string Title, decimal CreditUnits, Grade Grade);
